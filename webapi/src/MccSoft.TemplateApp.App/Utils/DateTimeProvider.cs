using System;

namespace MccSoft.TemplateApp.App.Utils;

/// <summary>
/// Provides a consistent value of "now" throughout a service operation.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; } = DateTime.UtcNow;
}

public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }
}
