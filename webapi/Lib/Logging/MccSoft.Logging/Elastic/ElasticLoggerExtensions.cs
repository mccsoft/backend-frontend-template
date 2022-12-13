using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Debugging;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;

namespace MccSoft.Logging;

public static class ElasticLoggerConfigurationExtensions
{
    /// <summary>
    /// A keyword has no internal structure, such fields are searched by exact match only.
    /// Example: field value "A B", query "A" will not find a match.
    /// </summary>
    private static readonly object _keyword = new { type = "keyword", ignore_above = 256 };

    /// <summary>
    /// A multifield that is a combination of a <see cref="_keyword" /> and a numeric field,
    /// allowing us to search by exact match like `f.Field: 111` and by comparing with a number, like
    /// `f.Field.num > 111`
    /// </summary>
    private static readonly object _keywordNumeric = new
    {
        type = "keyword",
        ignore_above = 256,
        fields = new { num = new { type = "long", ignore_malformed = true } }
    };

    private static object _long = new { type = "long" };
    private static object _float = new { type = "float" };
    private static object _date = new { type = "date" };

    private static object _boolean = new { type = "boolean" };

    /// <summary>
    /// A text field is "analyzed" on indexing, i.e. individual words are extracted and indexed, so partial matching is possible.
    /// Example: field value "A B", query "A" will find a match.
    /// </summary>
    private static readonly object _text = new
    {
        type = "text",
        // Do not store scoring metadata.
        norms = false,
        similarity = "boolean",
    };

    /// <summary>
    /// Configures Serilog to use Elasticsearch
    /// </summary>
    /// <param name="loggerConfiguration">Serilog default configuration.</param>
    /// <param name="configuration">Service configuration.</param>
    public static void WriteToElasticSearch(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration configuration
    )
    {
        var options = configuration.GetSection("Serilog:Elastic").Get<ElasticLoggingOptions>();
        if (
            string.IsNullOrEmpty(options.Url)
            || string.IsNullOrEmpty(options.User)
            || string.IsNullOrEmpty(options.Password)
        )
            return;

        Uri siteUri = new Uri(configuration.GetValue<string>("General:SiteUrl"));
        string stage = siteUri.Host;
        string serviceName = "TemplateApp".ToLower();
        bool initSchema = true;

        string replicaId = configuration.GetValue<string>("ReplicaId") ?? "1";

        var isoCulture = LoggerConfigurationExtensions.GetFormatProvider();

        var elasticSearchSinkOptions = new ElasticsearchSinkOptions(new Uri(options.Url))
        {
            // Default batch posting limit is 50, making us send batches too frequently in highly loaded services,
            // we trade a bit of memory for sending performance.
            BatchPostingLimit = 500,
            InlineFields = true,
            // Make each stage have its own set of indices, so if we change the "schema" or settings on one stage,
            // it doesn't affect settings on the other.
            // Make each service have its own index template and index name prefix to enable independent deployment.
            TemplateName = $"{serviceName}-{stage}---events-template",
            IndexFormat = $"{serviceName}--{stage}---" + "{0:yyyy.MM.dd}" + $"-{GetFieldsHash()}",
            CustomFormatter = CreateFormatter(isoCulture, isDurable: false),
            CustomDurableFormatter = CreateFormatter(isoCulture, isDurable: true),
            GetTemplateContent = () =>
                GetTemplateES(stage, templateMatchString: $"{serviceName}-{stage}---*"),
            AutoRegisterTemplate = initSchema,
            // Use FailSink to fail service if elastic is not available during startup.
            // We use auto registration only when we run infrastructure in this place we want to fail when elastic
            // is not available.
            RegisterTemplateFailure = RegisterTemplateRecovery.FailSink,
            // Replace the template on each service start to automatically update
            // static field mappings after release.
            OverwriteTemplate = initSchema,
            EmitEventFailure =
                EmitEventFailureHandling.WriteToFailureSink
                | EmitEventFailureHandling.WriteToSelfLog,
            FormatProvider = isoCulture,
        };
        // Disable sending "_type":"_doc" in bulk requests for Elastic v7 and higher.
        // See https://github.com/serilog-contrib/serilog-sinks-elasticsearch/blob/f14e29ffd8eb1b536efd9b5e8e150b60f49971ec/test/Serilog.Sinks.Elasticsearch.Tests/BulkActionTests.cs#L51
        elasticSearchSinkOptions.TypeName = null;

        elasticSearchSinkOptions.ModifyConnectionSettings = x =>
            x.BasicAuthentication(options.User, options.Password)
                // TODO: add a config flag to skip server certificate check
                // and a proper server certificate check.
                .ServerCertificateValidationCallback((o, certificate, chain, errors) => true);

        // The implementation of the file buffer is buggy in the Elasticsearch sink, it loses messages,
        // see https://github.com/serilog/serilog-sinks-elasticsearch/issues/382
        // albeit not too often, about 1 in 100k, so for low-load services
        // it's preferable to use in-memory buffering.
        if (options.EnableFileBuffer)
        {
            // Enable message durability.
            // Without durability, messages are accumulated in memory, and when the queue size reaches
            // ElasticsearchSinkOptions.QueueSizeLimit (100k by default) while waiting to ship them
            // to Elasticsearch, messages will be dropped.
            elasticSearchSinkOptions.BufferBaseFilename =
                $"./logs/{serviceName}@replica-{replicaId}/buffer";

            // Keep ~3GB of buffer files.
            elasticSearchSinkOptions.BufferFileCountLimit = 31;
            elasticSearchSinkOptions.BufferLogShippingInterval = TimeSpan.FromSeconds(2);
        }

        // Write errors that happen in the logger itself to the stderr and to a file, to enable two use cases:
        // 1. Immediately see lost (rejected by ES) messages in the output of `docker service logs service_name`.
        // 2. To keep errors related to lost messages in a file between service restarts.
        SelfLog.Enable(msg =>
        {
            Console.Error.WriteLine(msg);
            File.AppendAllLines(
                $"./logs/{serviceName}@replica-{replicaId}.selflog.txt",
                new[] { msg }
            );
        });

        // Use extended template for console messages to see properties from the scope.
        const string outputTemplate =
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";

        // If the stage is not local development stage - push logs to Elasticsearch.

        // We use deprecated FileSink as for now only this way of configure is supported.
#pragma warning disable CS0618 // Type or member is obsolete
        // Set Sink only in case if we add option to elastic,
        // because otherwise FileSink gets not released correctly inside of tests
        elasticSearchSinkOptions.FailureSink = new FileSink(
            $"./logs/{serviceName}@replica-{replicaId}.failure.txt",
            new CompactJsonFormatter(),
            fileSizeLimitBytes: null
        );
#pragma warning restore CS0618 // Type or member is obsolete

        loggerConfiguration.WriteTo.Logger(
            lc =>
                lc
                // Workaround for Override not working in sub-loggers,
                // see https://github.com/serilog/serilog/issues/1346
                // and https://github.com/serilog/serilog/issues/1453#issuecomment-654254454
                .MinimumLevel
                    .Verbose()
                    .ExcludeEfInformation()
                    .WriteTo.Elasticsearch(elasticSearchSinkOptions)
        );
    }

    private static ITextFormatter CreateFormatter(IFormatProvider formatProvider, bool isDurable)
    {
        return new PrefixRenameFormatter(
            formatProvider: formatProvider,
            closingDelimiter: isDurable ? Environment.NewLine : string.Empty,
            serializer: null,
            inlineFields: true,
            formatStackTraceAsArray: false,
            renderMessage: true
        );
    }

    /// <summary>
    /// Gets a custom index template for Serilog indices.
    /// </summary>
    /// <remarks>
    /// An index template defines settings and the "schema" of newly created Elasticsearch indices whose
    /// name matches <paramref name="templateMatchString" />.
    /// We use a custom template to force the type of certain fields (such as StatusCode) to have the type "text"
    /// to avoid type conflicts caused by libraries. For example, Microsoft logs StatusCode sometimes as 200,
    /// and sometimes as "OK". The text-formatted messages are rejected, because sometimes an integer-formatted
    /// message arrives first and defines the type of StatusCode in the index to be "integer".
    /// </remarks>
    /// <param name="stage">The current stage name (Host of the SiteUrl).</param>
    /// <param name="templateMatchString">
    /// The pattern to match the index name.
    /// Only when the pattern matches, the created template is applied to an index.
    /// </param>
    /// <returns>An object based on which the index template is created.</returns>
    private static object GetTemplateES(string stage, string templateMatchString)
    {
        // Based on
        // https://github.com/serilog/serilog-sinks-elasticsearch/blob/463c878a805153dc3f97f938c2aa46de0bc815e0/src/Serilog.Sinks.Elasticsearch/Sinks/ElasticSearch/ElasticSearchTemplateProvider.cs#L182
        object mappings = new
        {
            // Disable dynamic mapping of top-level fields.
            // This results in 3rd-party fields (logged in libs) not being indexed.
            dynamic = false,
            dynamic_templates = new List<object>
            {
                new
                {
                    obj_fields = new
                    {
                        match_mapping_type = "object",
                        path_match = Field.FieldPrefix + ".*",
                        mapping = new { type = "object" }
                    }
                },
                new
                {
                    keyword_fields = new
                    {
                        path_match = Field.FieldPrefix + ".*",
                        match_mapping_type = "*",
                        mapping = _keyword
                    }
                }
            },
            properties = new Dictionary<string, object>
            {
                {
                    // Fields defined via the `Field` class get a special prefix to distinguish
                    // them from 3rd party fields. We allow dynamic field creation of such prefixed fields only.
                    // If a top-level field (not having the prefix) is needed to be searchable, add a static
                    // mapping for it (see e.g. RequestPath below).
                    Field.FieldPrefix,
                    new { dynamic = true, properties = GetStaticFields() }
                },
                { "@timestamp", _date },
                { "SourceContext", _keyword },
                { "level", _keyword },
                { "message", _text },
                // We don't want to search in MessageTemplate, so don't index it.
                { "messageTemplate", new { type = "text", index = false } },
                { "SessionId", _keyword },
                { "TraceId", _keyword },
                { "ParentId", _keyword },
                { "SpanId", _keyword },
                { "MachineName", _keyword },
                { "ReplicaId", _keyword },
                { "ServiceName", _keyword },
                { "Version", _keyword },
                // Used by ASP.net
                { "ActionName", _keyword },
                { "Stage", _keyword },
                // Used by ASP.net to log the elapsed time of an http request
                { "ElapsedMilliseconds", _float },
                // Logged by MassTransit.
                { "MessageType", _keyword },
                {
                    "exceptions",
                    new
                    {
                        type = "nested",
                        properties = new Dictionary<string, object>
                        {
                            { "ClassName", _keyword },
                            { "Source", _keyword },
                            { "Depth", new { type = "integer" } },
                            { "RemoteStackIndex", new { type = "integer" } },
                            { "HResult", new { type = "integer" } },
                            { "StackTraceString", _text },
                            { "RemoteStackTraceString", _text },
                            { "Message", _text },
                            { "HelpURL", _keyword }
                        }
                    }
                },
                // In RequestPath we want to find words separated by slashes, so use a custom analyzer.
                {
                    "RequestPath",
                    new
                    {
                        type = "text",
                        norms = false,
                        // TODO (#20402): Uncomment when Elasticsearch is updated to v6.8 or later.
                        // similarity = "boolean",
                        analyzer = "non_word_char_splits_analyzer",
                        // Also store as a keyword to allow exact matches on values like "/".
                        // See https://www.elastic.co/guide/en/elasticsearch/reference/current/multi-fields.html
                        fields = new { raw = _keyword }
                    }
                },
                // MS logs sometimes 200, sometimes "OK".
                { "StatusCode", _keyword },
            }
        };

        object settings = new
        {
            // Default is 1s, i.e. refresh indices each second, which we don't need so often.
            // We refresh each more seldom to speed up indexing.
            refresh_interval = "10s",
            analysis = new
            {
                analyzer = new
                {
                    // The default analyzer splits words on whitespace, this custom analyzer splits on
                    // any non-word character. Useful to search words inside URLs.
                    non_word_char_splits_analyzer = new
                    {
                        type = "pattern",
                        pattern = "\\W+",
                        lowercase = true
                    }
                }
            },
            // Default is *, i.e. search in all fields (hundreds of them), which is too slow.
            query = new { default_field = new[] { "message" } }
        };
        var aliases = new Dictionary<string, object>();
        return new
        {
            index_patterns = new[] { templateMatchString },
            settings,
            mappings,
            aliases,
        };
    }

    /// <summary>
    /// Defines explicit (static) mappings for fields in Elasticsearch.
    /// All fields not marked with an <see cref="ElasticFieldAttribute"/> attribute will get their type dynamically,
    /// see the rules in `dynamic_templates` above.
    /// </summary>
    /// <returns>A dictionary (field name -> field config).</returns>
    private static Dictionary<string, object> GetStaticFields()
    {
        // Pre-fill static mappings that we don't configure via attributes in the Field class.
        var properties = new Dictionary<string, object>
        {
            // Configuration parameter values are logged as integers
            // and sometimes strings like "Parameters_Mode_mode_tmvc_CPAP".
            { "ParameterValue", _keyword },
        };

        object GetFieldConfig(FieldType t) =>
            t switch
            {
                FieldType.Text => _text,
                FieldType.Long => _long,
                FieldType.Keyword => _keyword,
                FieldType.Float => _float,
                FieldType.Date => _date,
                FieldType.KeywordNumeric => _keywordNumeric,
                FieldType.Boolean => _boolean,
                _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
            };

        IEnumerable<KeyValuePair<string, object>> fieldConfigs =
            from field in typeof(Field).GetFields(BindingFlags.Public | BindingFlags.Static)
            let typeAttr = field.GetCustomAttribute<ElasticFieldAttribute>()
            where typeAttr?.Type != null
            let fieldConfig = GetFieldConfig(typeAttr.Type)
            select new KeyValuePair<string, object>(field.Name, fieldConfig);
        foreach ((string fieldName, object conf) in fieldConfigs)
        {
            properties.Add(fieldName, conf);
        }

        return properties;
    }

    private static string GetFieldsHash()
    {
        var staticFields = GetStaticFields();
        var json = JsonConvert.SerializeObject(staticFields);

        using var md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(json);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        // Convert the byte array to hexadecimal string
        var sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("X2"));
        }

        return sb.ToString();
    }
}
