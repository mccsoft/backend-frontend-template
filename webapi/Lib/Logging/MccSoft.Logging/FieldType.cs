namespace MccSoft.Logging;

/// <summary>
/// Defines field types in Elasticsearch.
/// </summary>
public enum FieldType
{
    /// <summary>
    /// Suitable for full text search. Not suitable for sorting.
    /// </summary>
    Text,

    /// <summary>
    /// The field is found by exact match only. Range queries and sorting are done lexicographically.
    /// </summary>
    Keyword,

    /// <summary>
    /// Integer numbers. Suitable for range queries (e.g. f.MyField >10).
    /// </summary>
    Long,

    /// <summary>
    /// Numbers with a fractional part. Suitable for range queries (e.g. f.MyField >10.1).
    /// </summary>
    Float,

    /// <summary>
    /// Datetime type used for timestamp fields.
    /// </summary>
    Date,

    /// <summary>
    /// The field is found by exact match only, but has a "subfield" `.num`, range queries and sorting
    /// on which are done numerically. Make sure to only log strings in this field that are parseable to a number.
    /// </summary>
    KeywordNumeric,

    /// <summary>
    /// The field is found by match on the boolean flag.
    /// </summary>
    Boolean
}
