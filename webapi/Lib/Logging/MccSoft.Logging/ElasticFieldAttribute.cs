using System;

namespace MccSoft.Logging;

/// <summary>
/// Specifies explicit type the field should have in Elasticsearch.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ElasticFieldAttribute : Attribute
{
    public ElasticFieldAttribute(FieldType type)
    {
        Type = type;
    }

    /// <summary>
    /// The type with which the field is stored.
    /// </summary>
    public FieldType Type { get; private set; }
}
