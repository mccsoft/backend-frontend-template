using Microsoft.AspNetCore.Http;

namespace MccSoft.LowLevelPrimitives.Exceptions;

/// <summary>
/// Thrown when the resource requested by the client is not found and the server should
/// reply with 404.
/// </summary>
public class PersistenceResourceNotFoundException : WebApiBaseException
{
    public PersistenceResourceNotFoundException(string message)
        : base(
            ErrorTypes.NotFound,
            "The requested resource was not found.",
            StatusCodes.Status404NotFound,
            message
        ) { }
}
