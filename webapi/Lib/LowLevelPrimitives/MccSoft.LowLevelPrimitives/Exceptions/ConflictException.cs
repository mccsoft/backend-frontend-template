using Microsoft.AspNetCore.Http;

namespace MccSoft.LowLevelPrimitives.Exceptions
{
    public class ConflictException : WebApiBaseException
    {
        public ConflictException(string message)
            : base(
                ErrorTypes.Conflict,
                "Resource conflict occurred.",
                StatusCodes.Status409Conflict,
                message
            ) { }
    }
}
