namespace MccSoft.Logging;

public static class LoggingHeaders
{
    /// <summary>
    /// The name of the session-id header for propagation
    /// via <see cref="System.Diagnostics.Activity"/>.
    /// </summary>
    public const string SessionId = nameof(SessionId);
}
