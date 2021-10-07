using System;

namespace MccSoft.Mailing.Models
{
    public class MailSettings
    {
        public Func<EmailModelBase, EmailTemplateType, string> EmailViewPathProvider { get; set; }
    }
}
