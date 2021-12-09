namespace MccSoft.Logging
{
    public class RemoteLoggerOptions
    {
        /// <summary>
        /// Loggly token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Loggly server (usually logs-01.loggly.com)
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Loggly port (usually 443)
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Instance Name (usually something like 'dev/prod') that will be shown in logs as `InstanceName` field
        /// </summary>
        public string InstanceName { get; set; }
    }
}
