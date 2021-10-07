using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MccSoft.HttpClientExtension
{
    public static class ResponseExtension
    {
        /// <summary>
        /// Checks that the response message indicates success.
        /// Throws if it doesn't.
        /// </summary>
        public static async Task EnsureSuccess(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            string url = response.RequestMessage.RequestUri.ToString();
            string problemType = GetProblemType(response, responseContent);
            throw new FailedRequestException(
                response.StatusCode,
                responseContent,
                $"Error calling endpoint '{url}'. "
                    + $"Status code: {(int)response.StatusCode} ({response.StatusCode}). "
                    + $"Content: {responseContent}.",
                problemType
            );
        }

        /// <summary>
        /// Returns the problem type of the given response message.
        /// </summary>
        public static string GetProblemType(HttpResponseMessage response, string responseContent)
        {
            if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
            {
                try
                {
                    JObject o = JObject.Parse(responseContent);
                    return (string)o["type"] ?? "about:blank";
                }
                catch (JsonException) { }
            }

            return null;
        }
    }
}
