using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sentry;
using Sentry.Extensibility;

namespace MccSoft.WebApi.Sentry
{
    internal class SentryEventProcessor : ISentryEventProcessor
    {
        private readonly ILogger<SentryEventProcessor> _logger;
        private readonly string _assemblyName;
        private readonly string _stageName;

        public SentryEventProcessor(
            IConfiguration configuration,
            ILogger<SentryEventProcessor> logger
        )
        {
            _logger = logger;
            _stageName = configuration["Stage:Name"];
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                _assemblyName = entryAssembly.GetName().Name;
            }
        }

        public SentryEvent Process(SentryEvent @event)
        {
            if (!string.IsNullOrEmpty(_assemblyName))
            {
                @event.SetTag("serviceName", _assemblyName);
            }

            if (!string.IsNullOrEmpty(_stageName))
            {
                @event.SetTag("stage", _stageName);
            }

            string message = @event?.Message?.Message;
            if (!string.IsNullOrEmpty(message))
            {
                // For test errors, we additionally group this error
                // by message to prevent it from falling into breadcrumbs of other methods.
                if (message.Contains(SentryLoggerExtension.TestSentryMessagePrefix))
                {
                    @event.SetFingerprint(new[] { message });
                }

                // Group message bus consuming failures by service-message
                if (@event.Logger == "MccSoft.MessageBus.ReceiveObserverLogging")
                {
                    // HACK: Retrieve name of service bus message,
                    // which was failed to consume, from LogEntry.
                    string failedServiceBusMessageName = Regex
                        .Match(
                            message,
                            "Failed to consume \'(?<failedServiceBusMessageName>.*)\' in"
                        )
                        .Groups["failedServiceBusMessageName"].ToString();

                    @event.SetFingerprint(
                        new[]
                        {
                            $"ServiceName: {_assemblyName}",
                            $"FailedServiceBusMessage: {failedServiceBusMessageName}"
                        }
                    );
                }
            }
            else
            {
                _logger.LogError(
                    "Error logging message to Sentry. event.LogEntry?.Message is null."
                );
            }

            return @event;
        }
    }
}
