using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MccSoft.WebApi.Helpers
{
    public static class HttpContextExtensions
    {
        public static async Task<string> ReadAll(this HttpRequest request)
        {
            var buffer = await request.Body.ToArray();
            string requestBody = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);

            return requestBody;
        }
    }
}
