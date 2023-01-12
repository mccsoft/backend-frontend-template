using System.Collections.Generic;

namespace MccSoft.Logging;

/// <summary>
/// Context to contain parameters related to operation.
/// </summary>
public class OperationContext : Dictionary<Field, object> { }
