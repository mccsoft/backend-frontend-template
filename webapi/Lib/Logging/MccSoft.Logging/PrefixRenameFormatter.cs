using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;

namespace MccSoft.Logging
{
    /// <summary>
    /// Replaces a special underscore-containing prefix denoting a PrismaCloud field with
    /// a dot-containing prefix (in messages sent to Elasticsearch).
    /// This is a workaround for Serilog not allowing dots in message templates.
    /// See https://github.com/serilog/serilog/issues/1384
    /// </summary>
    public class PrefixRenameFormatter : ElasticsearchJsonFormatter
    {
        private const string _prefix = Field.FieldPrefix + "_";
        private const string _dottedPrefix = Field.FieldPrefix + ".";

        /// <summary>
        /// Construct a <see cref="T:Serilog.Formatting.Elasticsearch.PrefixRenameFormatter" />.
        /// </summary>
        /// <param name="omitEnclosingObject">
        /// If true, the properties of the event will be written to
        /// the output without enclosing braces. Otherwise, if false, each event will be written as a well-formed
        /// JSON object.
        /// </param>
        /// <param name="closingDelimiter">
        /// A string that will be written after each log event is formatted.
        /// If null, <see cref="P:System.Environment.NewLine" /> will be used.
        /// Ignored if <paramref name="omitEnclosingObject" />
        /// is true.
        /// </param>
        /// <param name="renderMessage">
        /// If true, the message will be rendered and written to the output as a
        /// property named RenderedMessage.
        /// </param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="serializer">Inject a serializer to force objects to be serialized over being ToString()</param>
        /// <param name="inlineFields">When set to true values will be written at the root of the json document</param>
        /// <param name="renderMessageTemplate">
        /// If true, the message template will be rendered and written to the output as a
        /// property named RenderedMessageTemplate.
        /// </param>
        /// <param name="formatStackTraceAsArray">
        /// If true, splits the StackTrace by new line and writes it as a an array of strings
        /// </param>
        public PrefixRenameFormatter(
            bool omitEnclosingObject = false,
            string closingDelimiter = null,
            bool renderMessage = true,
            IFormatProvider formatProvider = null,
            ISerializer serializer = null,
            bool inlineFields = false,
            bool renderMessageTemplate = true,
            bool formatStackTraceAsArray = false
        )
            : base(
                omitEnclosingObject,
                closingDelimiter,
                renderMessage,
                formatProvider,
                serializer,
                inlineFields,
                renderMessageTemplate,
                formatStackTraceAsArray
            ) { }

        /// <summary>
        /// Escape the name of the Property before calling ElasticSearch
        /// </summary>
        protected override void WriteDictionary(
            IReadOnlyDictionary<ScalarValue, LogEventPropertyValue> elements,
            TextWriter output
        )
        {
            Dictionary<ScalarValue, LogEventPropertyValue> escaped = elements.ToDictionary(
                e => ReplacePrefixInFieldName(e.Key),
                e => e.Value
            );

            base.WriteDictionary(escaped, output);
        }

        /// <summary>
        /// Escape the name of the Property before calling ElasticSearch
        /// </summary>
        protected override void WriteJsonProperty(
            string name,
            object value,
            ref string precedingDelimiter,
            TextWriter output
        )
        {
            name = ReplacePrefixInFieldName(name);

            base.WriteJsonProperty(name, value, ref precedingDelimiter, output);
        }

        /// <summary>
        /// Replaces the prefix in Strings and does nothing to objects
        /// </summary>
        protected virtual ScalarValue ReplacePrefixInFieldName(ScalarValue value)
        {
            return value.Value is string s ? new ScalarValue(ReplacePrefixInFieldName(s)) : value;
        }

        /// <summary>
        /// Replaces the prefix to Elasticsearch format (e.g. 'f_' -> 'f.').
        /// </summary>
        protected virtual string ReplacePrefixInFieldName(string value)
        {
            if (value == null)
            {
                return null;
            }

            // Field name is already stripped from the "@"-prefix, so we don't check for it.
            if (value.StartsWith(_prefix))
            {
                return _dottedPrefix + value.Substring(_prefix.Length);
            }

            return value;
        }

        protected override void WriteMessageTemplate(
            string template,
            ref string delim,
            TextWriter output
        )
        {
            string escaped = template
                .Replace("{" + _prefix, "{" + _dottedPrefix)
                .Replace("{@" + _prefix, "{@" + _dottedPrefix);
            base.WriteMessageTemplate(escaped, ref delim, output);
        }
    }
}
