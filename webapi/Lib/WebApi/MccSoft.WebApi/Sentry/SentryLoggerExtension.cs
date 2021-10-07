using Microsoft.Extensions.Logging;

namespace MccSoft.WebApi.Sentry
{
    public static class SentryLoggerExtension
    {
        public const string TestSentryMessagePrefix =
            "Test error to check Sentry integration from ";

        /// <summary>
        /// After service starts we need some test error event to be sent in Sentry.
        /// This event will ensure that Sentry reporting works for this service.
        /// </summary>
        public static void LogSentryTestError(this ILogger logger, string serviceName)
        {
            logger.LogError($"{TestSentryMessagePrefix}{serviceName}.");
        }
    }
}
