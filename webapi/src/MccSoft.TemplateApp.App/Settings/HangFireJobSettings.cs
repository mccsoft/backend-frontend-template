namespace MccSoft.TemplateApp.App.Settings
{
    /// <summary>
    /// Represents startup options for HangFire job.
    /// </summary>
    public abstract class HangFireJobSettings
    {
        /// <summary>
        /// Should be a cron compatible expression that is passed to hangfire scheduler.
        /// </summary>
        public string CronExpression { get; set; }
    }
}
