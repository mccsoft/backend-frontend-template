namespace MccSoft.WebApi.Throttler;

/// <summary>
/// Request will be defined by such unique entity, called ThrottleGroup.
/// </summary>
public static class ThrottleGroup
{
    /// <summary>
    /// Identity field of authorized user.
    /// </summary>
    public const string Identity = nameof(Identity);

    /// <summary>
    /// IP address of client.
    /// </summary>
    public const string IpAddress = nameof(IpAddress);
}
