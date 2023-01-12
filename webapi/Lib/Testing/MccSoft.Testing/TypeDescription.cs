namespace MccSoft.Testing;

/// <summary>
/// Contains the summary XML-documentation of a type.
/// </summary>
public class TypeDescription
{
    public TypeDescription(string type, string description)
    {
        Type = type;
        Description = description;
    }

    /// <summary>
    /// Name of the Type
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// The description of the type
    /// </summary>
    public string Description { get; }
}
