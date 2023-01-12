namespace MccSoft.WebApi.Throttler;

/// <summary>
/// Options for Throttler.
/// </summary>
public class ThrottleConfigOptions
{
    /// <summary>
    /// Number of requests allowed during <see cref="TimeoutInSeconds"/>.
    /// If there will be more than <see cref="RequestLimit"/> during <see cref="TimeoutInSeconds"/> seconds
    /// requests will start to fail with HTTP 429.
    /// </summary>
    public int RequestLimit { get; set; }

    /// <summary>
    /// Time limit after which the request quotas are reset
    /// (i.e. it's allowed to make another <see cref="RequestLimit"/> number of requests).
    /// </summary>
    public int TimeoutInSeconds { get; set; }
}
