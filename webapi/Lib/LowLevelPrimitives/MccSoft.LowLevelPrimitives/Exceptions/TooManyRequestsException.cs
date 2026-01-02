using System;
using Microsoft.AspNetCore.Http;

namespace MccSoft.LowLevelPrimitives.Exceptions;

/// <summary>
/// Thrown when the client has sent too many requests in a given amount of time and the server should
/// reply with 429.
/// </summary>
public sealed class TooManyRequestsException : WebApiBaseException
{
    public int? RetryAfterSeconds { get; }
    public DateTime? LockedUntilUtc { get; }

    public TooManyRequestsException(
        string message,
        int? retryAfterSeconds = null,
        DateTime? lockedUntil = null
    )
        : base(
            ErrorTypes.TooManyRequests,
            "Too many requests.",
            StatusCodes.Status429TooManyRequests,
            message
        )
    {
        RetryAfterSeconds = retryAfterSeconds;
        LockedUntilUtc = lockedUntil;
    }
}
