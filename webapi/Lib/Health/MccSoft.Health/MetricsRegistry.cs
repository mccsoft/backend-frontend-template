using App.Metrics;
using App.Metrics.Gauge;
using App.Metrics.Timer;

namespace MccSoft.Health
{
    /// <summary>
    /// Defines custom application metrics.
    /// </summary>
    public static class MetricsRegistry
    {
        /// <summary>
        /// A timer to measure the rate and duration of handling of events
        /// received via the service bus.
        /// </summary>
        public static TimerOptions EventHandlingDuration { get; } =
            new TimerOptions
            {
                Name = "Event handling duration",
                MeasurementUnit = Unit.Events,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Minutes
            };

        /// <summary>
        /// A timer to measure the rate and duration of handling of events
        /// received via the service bus. The measurement is done separately for each event type.
        /// </summary>
        public static TimerOptions EventHandlingDurationByType { get; } =
            new TimerOptions
            {
                Name = "Event handling duration by type",
                MeasurementUnit = Unit.Events,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Minutes
            };

        /// <summary>
        /// A gauge that counts the amount of daily statistics in Core and other services
        /// that store daily statistics.
        /// </summary>
        public static GaugeOptions DailyStatisticsRecordCount { get; } =
            new GaugeOptions
            {
                Name = "Count of real daily statistic records",
                MeasurementUnit = Unit.Items
            };

        /// <summary>
        /// A gauge that counts the amount of daily statistics in Core created for tests
        /// and other services that store daily statistics.
        /// </summary>
        public static GaugeOptions TestDailyStatisticsRecordCount { get; } =
            new GaugeOptions
            {
                Name = "Count of daily statistic records created with test API",
                MeasurementUnit = Unit.Items
            };
    }
}
