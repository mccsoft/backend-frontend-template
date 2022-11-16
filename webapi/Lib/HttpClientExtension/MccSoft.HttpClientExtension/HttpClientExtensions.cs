using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MccSoft.HttpClientExtension
{
    /// <summary>
    /// Contains helper methods extending the <see cref="HttpClient"/> class.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Sends a POST request to the specified URL with the specified data as the body.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to.</typeparam>
        /// <param name="client">The HTTP client.</param>
        /// <param name="url">The endpoint to send the request to.</param>
        /// <param name="data">The body of the request.</param>
        /// <returns>The response deserialized to the <typeparamref name="T"/> type.</returns>
        public static async Task<T> PostAsJson<T>(this HttpClient client, string url, object data)
        {
            string json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await client.PostAsync(url, content);
            return await ReadResponse<T>(response);
        }

        /// <summary>
        /// Sends a PUT request to the specified URL with the specified data as the body.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to.</typeparam>
        /// <param name="client">The HTTP client.</param>
        /// <param name="url">The endpoint to send the request to.</param>
        /// <param name="data">The body of the request.</param>
        /// <returns>The response deserialized to the <typeparamref name="T"/> type.</returns>
        public static async Task<T> PutAsJson<T>(this HttpClient client, string url, object data)
        {
            string json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await client.PutAsync(url, content);
            return await ReadResponse<T>(response);
        }

        /// <summary>
        /// Sends a GET request to the specified URL.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to.</typeparam>
        /// <param name="client">The HTTP client.</param>
        /// <param name="url">The endpoint to send the request to.</param>
        /// <returns>The response deserialized to the <typeparamref name="T"/> type.</returns>
        public static async Task<T> GetAsJson<T>(this HttpClient client, string url)
        {
            using HttpResponseMessage response = await client.GetAsync(url);
            return await ReadResponse<T>(response);
        }

        /// <summary>
        /// Sends a PUT request to the specified URL with the specified data as the body.
        /// </summary>
        /// <param name="client">The HTTP client.</param>
        /// <param name="url">The endpoint to send the request to.</param>
        /// <param name="data">The body of the request.</param>
        public static async Task PutAsJson(this HttpClient client, string url, object data)
        {
            string json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await client.PutAsync(url, content);
            await response.EnsureSuccess();
        }

        /// <summary>
        /// Sends a PATCH request to the specified URL with the specified data as the body.
        /// </summary>
        /// <param name="client">The HTTP client.</param>
        /// <param name="url">The endpoint to send the request to.</param>
        /// <param name="data">The body of the request.</param>
        public static async Task PatchAsJson(this HttpClient client, string url, object data)
        {
            string json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await client.PatchAsync(url, content);
            await response.EnsureSuccess();
        }

        private static async Task<T> ReadResponse<T>(HttpResponseMessage response)
        {
            await response.EnsureSuccess();
            string body = await response.Content.ReadAsStringAsync();

            T result;
            try
            {
                result = JsonSerializer.Deserialize<T>(body);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"Could not read the response. {e.Message} Content: '{body}'.",
                    e
                );
            }

            return result;
        }
    }
}
