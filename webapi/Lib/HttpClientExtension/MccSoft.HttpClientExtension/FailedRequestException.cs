using System;
using System.Net;

namespace MccSoft.HttpClientExtension
{
    /// <summary>
    /// Thrown when the HTTP server responds with an unsuccessful status code.
    /// </summary>
    public class FailedRequestException : Exception
    {
        private const string _urnPrefix = "urn:mccsoft:";

        public FailedRequestException(
            HttpStatusCode statusCode,
            string content,
            string message,
            string problemType
        ) : base(message)
        {
            StatusCode = statusCode;
            Content = content;
            ProblemType = problemType;
        }

        /// <summary>
        /// Checks that the error response indicates a problem created by our code.
        /// </summary>
        public bool IsOurProblem() => ProblemType?.StartsWith(_urnPrefix) == true;

        /// <summary>
        /// The status code of the HTTP response.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// The content of the HTTP response.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the 'type' property of the ProblemDetails object that the server returned.
        /// Returns <c>null</c> if problem details could not be extracted from the response.
        /// See https://tools.ietf.org/html/rfc7807#section-3.1
        /// </summary>
        public string ProblemType { get; }
    }
}
