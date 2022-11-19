using System;

namespace MccSoft.Logging;

/// <summary>
/// Represents commonly used logging fields,
/// to avoid different names for the same thing.
/// </summary>
public class Field
{
    /// <summary>
    /// A prefix added to fields, making it possible to enable dynamic mapping in Elasticsearch
    /// only for them. All other fields are made searchable only via static mapping.
    /// </summary>
    internal const string FieldPrefix = "f";

    #region Common log fields.

    /// <summary>
    /// The operation duration (in milliseconds).
    /// The type of this field is numeric.
    /// </summary>
    /// <remarks>
    /// Enables us to analyze performance of a certain operation, and also make
    /// range queries to this field (e.g. find all operations that took longer than 100ms).
    /// </remarks>
    [ElasticField(FieldType.Long)]
    public static Field Elapsed = new(nameof(Elapsed));

    /// <summary>
    /// The tenant (Organisation name).
    /// </summary>
    public static Field Tenant = new(nameof(Tenant));

    /// <summary>
    /// Represents name of the method (Operation name basically).
    /// </summary>
    public static Field Method = new(nameof(Method));

    /// <summary>
    /// A generic error message, e.g. extracted from an exception.
    /// </summary>
    /// <remarks>
    /// Use this field for logging arbitrary error messages that may contain substrings that look
    /// like placeholders but aren't, e.g. "{bad} {bad}".
    /// Consider also using <seealso cref="Exception"/> if it's OK to log the entire exception
    /// (with the call stack).
    /// </remarks>
    public static Field ErrorMessage = new(nameof(ErrorMessage));

    /// <summary>
    /// Some .NET Exception.
    /// </summary>
    public static Field Exception = new(nameof(Exception));

    #endregion

    private readonly string _nameForTemplate;

    private Field(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        PrefixedName =
            name[0] == '@' ? $"@{FieldPrefix}_{Name.Substring(1)}" : $"{FieldPrefix}_{Name}";
        _nameForTemplate = $"{{{PrefixedName}}}";
    }

    /// <summary>
    /// The (non-prefixed) name of the field, as seen by the developer.
    /// </summary>
    internal string Name { get; }

    /// <summary>
    /// The prefixed name, see <see cref="FieldPrefix"/>.
    /// </summary>
    internal string PrefixedName { get; }

    /// <summary>
    /// Create an ad hoc field that is logged only at one place, so a conventional field
    /// name is not needed.
    /// </summary>
    /// <returns>The created field.</returns>
    public static Field Named(string name)
    {
        return new(name);
    }

    public override string ToString()
    {
        return _nameForTemplate;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Field)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    protected bool Equals(Field other)
    {
        return Name == other.Name;
    }
}
