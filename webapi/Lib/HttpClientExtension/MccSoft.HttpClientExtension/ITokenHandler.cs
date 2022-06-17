using System.Threading.Tasks;

namespace MccSoft.HttpClientExtension;

public interface ITokenHandler
{
    Task<string> GetAccessToken();
    Task<string> RefreshToken();
}
