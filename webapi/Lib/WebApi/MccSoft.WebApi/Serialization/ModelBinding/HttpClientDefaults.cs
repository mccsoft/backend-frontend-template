using System.Net.Http;
using System.Net.Http.Formatting;

namespace MccSoft.WebApi.Serialization.ModelBinding;

/// <summary>
/// Makes it possible to change default deserialization settings for HttpClient.
/// Required to deserialize all dates with DateTimeKind.Utc
/// </summary>
public class HttpClientDefaults
{
    public static MediaTypeFormatterCollection MediaTypeFormatters
    {
        get
        {
            var p = typeof(HttpContentExtensions).GetProperty(
                "DefaultMediaTypeFormatterCollection",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
            return (MediaTypeFormatterCollection)p.GetValue(null, null);
        }
    }
}
