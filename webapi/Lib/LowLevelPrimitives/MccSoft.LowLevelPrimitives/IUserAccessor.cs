namespace MccSoft.LowLevelPrimitives;

/// <summary>
/// interface for getting identification information about current user
/// </summary>
public partial interface IUserAccessor
{
    /// <summary>
    /// returns Id of current user; throws if user is not authenticated or we are out of http request context
    /// </summary>
    string GetUserId();

    /// <summary>
    /// returns true if we are in scope of http request; false otherwise
    /// </summary>
    bool IsHttpContextAvailable { get; }

    /// <summary>
    /// returns true if we are in scope of http request and user is authenticated; false otherwise
    /// </summary>
    bool IsUserAuthenticated { get; }
}
