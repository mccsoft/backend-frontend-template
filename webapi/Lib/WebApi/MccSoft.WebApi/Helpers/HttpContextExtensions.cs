using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MccSoft.WebApi.Helpers
{
    public static class HttpContextExtensions
    {
        public static string ReadAll(this HttpRequest request)
        {
            var buffer = request.Body.ToArray();
            string requestBody = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);
            
            return requestBody;
        }
    }
}