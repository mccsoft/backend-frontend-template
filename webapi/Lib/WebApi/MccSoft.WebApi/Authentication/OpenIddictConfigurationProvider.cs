using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace MccSoft.WebApi.Authentication;

public class OpenIddictConfigurationProvider : IOpenIddictConfigurationProvider
{
    private readonly Dictionary<string, OpenIddictClientConfiguration> _clients;

    public OpenIddictConfigurationProvider(IOptions<OpenIddictConfiguration> configuration)
    {
        _clients = configuration.Value.Clients.ToDictionary(x => x.ClientId);
    }

    public OpenIddictClientConfiguration GetConfiguration(string clientId)
    {
        return _clients[clientId];
    }

    public bool TryGetConfiguration(
        string clientId,
        out OpenIddictClientConfiguration configuration
    )
    {
        return _clients.TryGetValue(clientId, out configuration);
    }

    public IList<OpenIddictClientConfiguration> GetAllConfigurations()
    {
        return _clients.Values.ToList();
    }
}
