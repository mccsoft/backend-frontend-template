namespace MccSoft.Logging;

public class ElasticLoggingOptions
{
    public string Url { get; set; }
    public string User { get; set; }
    public string Password { get; set; }

    public bool EnableFileBuffer { get; set; }
}
