namespace MccSoft.Logging;

public class ElasticLoggingOptions
{
    public string Url { get; set; }
    public string User { get; set; }
    public string Password { get; set; }

    /// <summary>
    /// Optional parameter to override the name of the index in OpenSearch.
    /// Defaults to the <see cref="User"/>.
    /// </summary>
    public string IndexName { get; set; }

    public bool EnableFileBuffer { get; set; }
}
