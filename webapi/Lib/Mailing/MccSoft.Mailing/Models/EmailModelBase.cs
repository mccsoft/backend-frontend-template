namespace MccSoft.Mailing.Models
{
    public class EmailModelBase
    {
        // You don't need to fill the fields of base EmailModel manually.
        // They will be filled by EmailSender automatically.
        // Just use them in your template and don't worry :)

        public string SiteRootUrl { get; set; }
        public string RecipientEmail { get; set; }
        public string Subject { get; set; }
    }
}
