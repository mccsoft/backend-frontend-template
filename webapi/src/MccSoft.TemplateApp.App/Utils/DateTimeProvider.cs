using MccSoft.LowLevelPrimitives.Extensions;

namespace MccSoft.TemplateApp.App.Utils;

/// <summary>
/// Provides a consistent value of "now" throughout a service operation.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTimeOffset UtcNowOffset { get; } = DateTimeOffset.UtcNow;
}

public interface IDateTimeProvider
{
    /// <summary>
    /// Current date and time by UTC on server.
    /// </summary>
    public DateTimeOffset UtcNowOffset { get; }

    /// <summary>
    /// Current date and time by UTC on server.
    /// </summary>
    public DateTime UtcNow => UtcNowOffset.UtcDateTime;

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
    private Func<DateTimeOffset> _dateTimeOffsetFactory;

    public Func<DateTimeOffset> DateTimeOffsetFactory
    {
        get => _dateTimeOffsetFactory;
        set
        {
            if (value().Offset != TimeSpan.Zero)
                throw new ArgumentException("Factory must return time in UTC.", nameof(value));
            _dateTimeOffsetFactory = value;
        }
    }

    public TestDateTimeProvider(DateTimeOffset dateTimeOffset)
    {
        if (dateTimeOffset.Offset != TimeSpan.Zero)
            throw new ArgumentException(
                $"{nameof(dateTimeOffset)} must be in UTC.",
                nameof(dateTimeOffset)
            );

        _dateTimeOffsetFactory = () => dateTimeOffset;
    }

    public TestDateTimeProvider(Func<DateTimeOffset> dateTimeOffsetFactory)
    {
        if (dateTimeOffsetFactory().Offset != TimeSpan.Zero)
            throw new ArgumentException(
                "Factory must return time in UTC.",
                nameof(dateTimeOffsetFactory)
            );

        _dateTimeOffsetFactory = dateTimeOffsetFactory;
    }

    /// <inheritdoc />
    public DateTimeOffset UtcNowOffset => DateTimeOffsetFactory();
}
