using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MccSoft.WebApi.Helpers;

public static class HttpContextExtensions
{
    public static async Task<string> ReadAll(this HttpRequest request)
    {
        var buffer = await request.Body.ToArray();
        string requestBody = Encoding.UTF8.GetString(buffer);
        request.Body.Seek(0, SeekOrigin.Begin);

        return requestBody;
    }

    public static void SetCache(this HttpResponse response, int maxAge, params string[] varyBy)
    {
        if (maxAge == 0)
        {
            SetNoCache(response);
        }
        else if (maxAge > 0)
        {
            if (!response.Headers.ContainsKey("Cache-Control"))
            {
                response.Headers.Add("Cache-Control", $"max-age={maxAge}");
            }

            if (varyBy?.Any() == true)
            {
                var vary = varyBy.Aggregate((x, y) => x + "," + y);
                if (response.Headers.ContainsKey("Vary"))
                {
                    vary = response.Headers["Vary"].ToString() + "," + vary;
                }
                response.Headers["Vary"] = vary;
            }
        }
    }

    public static void SetNoCache(this HttpResponse response)
    {
        if (!response.Headers.ContainsKey("Cache-Control"))
        {
            response.Headers.Add("Cache-Control", "no-store, no-cache, max-age=0");
        }
        else
        {
            response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
        }

        if (!response.Headers.ContainsKey("Pragma"))
        {
            response.Headers.Add("Pragma", "no-cache");
        }
    }
}
