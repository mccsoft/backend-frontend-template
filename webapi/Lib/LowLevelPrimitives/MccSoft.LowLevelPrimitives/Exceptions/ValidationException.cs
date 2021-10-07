using System;
using System.Collections.Generic;
using System.Linq;
using MccSoft.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MccSoft.LowLevelPrimitives.Exceptions
{
    /// <summary>
    /// Thrown when the client provides an invalid input and the
    /// server should respond with a 400 status.
    /// </summary>
    public class ValidationException : ApplicationException, IWebApiException, INoSentryException
    {
        private readonly ValidationProblemDetails _details;

        public ValidationException(string message) : base(message)
        {
            _details = new ValidationProblemDetails
            {
                Type = ErrorTypes.ValidationError,
                Detail = message,
                Instance = "",
            };
        }

        public ValidationException(IDictionary<string, string[]> errors)
            : base(ErrorsToString(errors))
        {
            _details = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = ErrorTypes.ValidationError,
                Detail = "",
                Instance = "",
            };
            foreach (KeyValuePair<string, string[]> e in errors)
            {
                _details.Errors[e.Key] = e.Value;
            }
        }

        public ValidationException(string fieldName, string errorMessage)
            : this(new Dictionary<string, string[]> { { fieldName, new[] { errorMessage } } }) { }

        /// <inheritdoc />
        public IActionResult Result =>
            new ObjectResult(_details)
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ContentTypes = { "application/problem+json" }
            };

        private static string ErrorsToString(IDictionary<string, string[]> errors)
        {
            IEnumerable<string> errs = from kv in errors from v in kv.Value select $"{kv.Key}: {v}";
            return string.Join("\n", errs);
        }
    }
}
