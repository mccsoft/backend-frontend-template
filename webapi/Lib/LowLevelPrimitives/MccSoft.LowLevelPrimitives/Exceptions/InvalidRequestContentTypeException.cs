using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace MccSoft.LowLevelPrimitives.Exceptions;

/// <summary>
/// Thrown when the device sends a invalid Http Content-Type
/// </summary>
public class InvalidRequestContentTypeException : Exception, IWebApiException
{
    public InvalidRequestContentTypeException(string message) : base(message) { }

    public IActionResult Result
    {
        get
        {
            var cr = new ContentResult
            {
                Content = Message,
                StatusCode = (int?)HttpStatusCode.BadRequest
            };
            return cr;
        }
    }
}
