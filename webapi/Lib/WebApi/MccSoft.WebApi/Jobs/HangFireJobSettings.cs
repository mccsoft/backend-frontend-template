namespace MccSoft.WebApi.Jobs;

/// <summary>
/// Represents startup options for HangFire job.
/// </summary>
public abstract class HangFireJobSettings
{
    /// <summary>
    /// Should be a cron compatible expression that is passed to hangfire scheduler.
    /// </summary>
    public string CronExpression { get; set; }

    /// <summary>
    /// Allows to disable the job (if set to false).
    /// If the value is `true` or `null` the job is enabled.
    /// </summary>
    public bool? IsEnabled { get; set; }
}
