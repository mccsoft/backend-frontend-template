using System;

namespace MccSoft.LowLevelPrimitives.Exceptions
{
    /// <summary>
    /// This exception could be thrown from Domain entities, and will result in HTTP 403
    /// </summary>
    public class PersistenceAccessDeniedException : Exception
    {
        public PersistenceAccessDeniedException(string message) : base(message) { }
    }
}
