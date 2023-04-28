using MccSoft.LowLevelPrimitives.Extensions;

namespace MccSoft.TemplateApp.App.Utils;

/// <summary>
/// Provides a consistent value of "now" throughout a service operation.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow { get; } = DateTime.UtcNow;
}

public interface IDateTimeProvider
{
    /// <summary>
    /// Current date and time by UTC on server.
    /// </summary>
    public DateTime UtcNow { get; }

    /// <summary>
    /// Current date by UTC on server
    /// </summary>
    public DateOnly UtcToday => UtcNow.ToDateOnly();
}

/// <summary>
/// Implementation of IDateTimeProvider for testing purpose.
/// Prefer this class over using <c>Mock&lt;IDateTimeProvider&gt;</c>
/// to reduce setup code.
/// </summary>
public sealed class TestDateTimeProvider : IDateTimeProvider
{
    // ReSharper disable NotNullOrRequiredMemberIsNotInitialized

    public TestDateTimeProvider(DateTime utcNow) => UtcNow = utcNow;

    public TestDateTimeProvider(Func<DateTime> utcNowFactory) => UtcNowFactory = utcNowFactory;

    // ReSharper restore NotNullOrRequiredMemberIsNotInitialized

    private Func<DateTime> _utcNowFactory;

    public Func<DateTime> UtcNowFactory
    {
        get => _utcNowFactory;
        set
        {
            if (value().Kind != DateTimeKind.Utc)
                throw new ArgumentException("Factory must return time in UTC.", nameof(value));
            _utcNowFactory = value;
        }
    }

    /// <inheritdoc />
    public DateTime UtcNow
    {
        get => _utcNowFactory();
        set
        {
            if (value.Kind != DateTimeKind.Utc)
                throw new ArgumentException($"UtcNow must be in UTC.", nameof(value));

            _utcNowFactory = () => value;
        }
    }
}
