using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace MccSoft.Testing
{
    /// <summary>
    /// A message logged by a mock logger.
    /// </summary>
    /// <remarks>
    /// See <see cref="AppServiceBaseTest{TService}.LoggedMessages"/>.
    /// </remarks>
    public class LoggedMessage
    {
        /// <summary>
        /// Level of the message
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// The message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// All log values.
        /// <see cref="FormattedLogValues"/>
        /// <c>null</c> in case if the values not exists
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, object>> LogValues { get; set; }
    }
}
