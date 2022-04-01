namespace MccSoft.TemplateApp.App.Settings
{
    /// <summary>
    /// Defines Swagger section of appsettings.json configuration
    /// </summary>
    public class SwaggerOptions
    {
        public class LicenseOptions
        {
            public string? Name { get; set; }
        }

        public class EndpointOptions
        {
            public string? Url { get; set; }
            public string? Name { get; set; }
        }

        public class ContactOptions
        {
            public string? Email { get; set; }
        }

        public bool Enabled { get; set; }

        public string? Description { get; set; }
        public string? Title { get; set; }
        public string? Version { get; set; }
        public string? BasePath { get; set; }
        public string? ClientPublicKey { get; set; }

        public ContactOptions Contact { get; set; } = new ContactOptions();
        public EndpointOptions Endpoint { get; set; } = new EndpointOptions();
        public LicenseOptions License { get; set; } = new LicenseOptions();
    }
}
