using System.Collections.Generic;

namespace MccSoft.Logging;

public sealed class SerilogRequestLoggingOptions
{
    public static string Position => "Serilog:RequestLogging";

    /// <summary>
    /// Requests whose path starts from one of this values
    /// will be excluded from logs
    /// </summary>
    public List<string> ExcludePaths { get; set; } = new();
}
