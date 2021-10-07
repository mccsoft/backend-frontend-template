namespace MccSoft.Mailing.Models
{
    /// <summary>
    /// Options to configure SMTP transport (e.g. via appsettings.json)
    /// </summary>
    public class MailSenderOptions
    {
        /// <summary>
        /// URL of the stage the site is currently running on.
        /// Required for building correct links within emails
        /// </summary>
        public string SiteUrl { get; set; }

        /// <summary>
        /// SMTP server host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// SMTP server port
        /// </summary>
        public int Port { get; set; }

        public bool IsSecureConnection { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// Email of sender
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Text description of sender. E.g. 'MCC Soft' (in 'MCC Soft <info@mcc-tomsk.de>')
        /// </summary>
        public string FromName { get; set; }
    }
}
