namespace MccSoft.LowLevelPrimitives;

/// <summary>
/// Defines URIs for various types of errors, as required by
/// the Problem Details spec, see https://tools.ietf.org/html/rfc7807#section-3.1
/// </summary>
/// <remarks>
/// These should be resolvable to human-readable web-pages, but we don't have such pages,
/// therefore we specify URNs, see https://en.wikipedia.org/wiki/Uniform_Resource_Name
/// (in short, URNs are like URLs, but are not locators and are not associated with particular
/// resolution or access protocols).
/// </remarks>
public static class ErrorTypes
{
    private const string _namespace = "urn:mccsoft:";

    public const string NotFound = _namespace + "not-found";
    public const string ValidationError = _namespace + "validation-error";
    public const string AccessDenied = _namespace + "access-denied";
    public const string InternalServerError = _namespace + "internal-server-error";
    public const string TooManyRequests = _namespace + "too-many-requests";
    public const string Conflict = _namespace + "conflict";

    public static string PrependNamespace(string errorType)
    {
        return _namespace + errorType;
    }
}
