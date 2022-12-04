namespace MccSoft.Logging;

/// <summary>
/// Represents commonly used logging fields,
/// to avoid different names for the same thing.
/// Put project-specific fields here.
/// </summary>
public partial class Field
{
    public static Field ProductId = new(nameof(ProductId));
}
