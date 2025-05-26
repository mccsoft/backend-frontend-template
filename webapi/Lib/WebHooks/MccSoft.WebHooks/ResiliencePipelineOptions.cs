using System;
using Polly;

namespace MccSoft.WebHooks;

public class ResiliencePipelineOptions
{
    /// <summary>
    /// Gets or sets the base delay between retries.
    /// </summary>
    /// <value>
    /// The default value is 2 seconds
    /// </value>
    public TimeSpan Delay { get; set; }

    /// <summary>
    /// Gets or sets the type of the back-off.
    /// </summary>
    /// <value>
    /// The default value is <see cref="DelayBackoffType.Exponential"/>
    /// </value>
    public DelayBackoffType BackoffType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether jitter should be used when calculating the backoff delay between retries.
    /// </summary>
    /// <value>
    /// The default value is true
    /// </value>
    public bool UseJitter { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retries to use Polly. After these retries, it will be moved to hangfire
    /// </summary>
    /// <value>
    /// The default value is 5
    /// </value>
    public int MaxRetryAttempts { get; set; }

    /// <summary>
    /// Adds a timeout to the builder.
    /// </summary>
    /// <value>
    /// The default value is 30 seconds
    /// </value>
    public TimeSpan Timeout { get; set; }
}
