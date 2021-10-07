using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;

namespace MccSoft.Testing
{
    /// <summary>
    /// Base class for application test (Without DB)
    /// </summary>
    public abstract class AppServiceBaseTest<TService> : TestBase
    {
        /// <summary>
        /// Gets the list of messages logged by <see cref="Sut"/>.
        /// For proper registering of log messages, SUT must be configured
        /// with the logger returned from <see cref="SetupLogger"/>.
        /// </summary>
        public List<LoggedMessage> LoggedMessages { get; } = new List<LoggedMessage>();

        /// <summary>
        /// Gets the service being tested (System Under Test).
        /// </summary>
        protected TService Sut { get; set; }

        /// <summary>
        /// Gets a mocked logger that appends messages to <see cref="LoggedMessages"/>.
        /// </summary>
        public ILogger<TAnyService> SetupLogger<TAnyService>()
        {
            var loggerMock = new Mock<ILogger<TAnyService>>();
            loggerMock.Setup(
                    logger =>
                        logger.Log(
                            It.IsAny<LogLevel>(),
                            It.IsAny<EventId>(),
                            It.IsAny<It.IsAnyType>(),
                            It.IsAny<Exception>(),
                            // https://github.com/dotnet/extensions/issues/1319
                            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                        )
                )
                .Callback(
                    (LogLevel level, EventId eId, object state, Exception ex, object formatter) =>
                    {
                        LoggedMessages.Add(
                            new LoggedMessage
                            {
                                Level = level,
                                LogValues = state as IReadOnlyList<KeyValuePair<string, object>>,
                                Message = state.ToString()
                            }
                        );
                    }
                );
            return loggerMock.Object;
        }
    }
}
