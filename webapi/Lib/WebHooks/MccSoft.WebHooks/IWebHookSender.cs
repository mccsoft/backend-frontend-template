using System.Collections.Generic;
using System.Threading.Tasks;

public interface IWebHookSender
{
    Task SendWebHook(
        string method,
        string url,
        string? body = null,
        Dictionary<string, string>? headers = null
    );
}
