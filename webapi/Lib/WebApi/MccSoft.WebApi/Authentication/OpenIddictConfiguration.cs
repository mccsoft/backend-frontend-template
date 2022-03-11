using OpenIddict.Abstractions;

namespace MccSoft.WebApi.Authentication;

public class OpenIddictConfiguration
{
    public OpenIddictClientConfiguration[] Clients { get; set; }
}

public class OpenIddictClientConfiguration : OpenIddictApplicationDescriptor
{
    public int? AccessTokenLifetime { get; set; }
    public int? RefreshTokenLifetime { get; set; }
}
