using System.IO;
using System.Threading.Tasks;

namespace MccSoft.WebApi.Helpers;

public static class StreamExtensions
{
    public static async Task<byte[]> ToArray(this Stream stream)
    {
        long? position = null;
        if (stream.CanSeek)
        {
            position = stream.Position;
            stream.Seek(0, SeekOrigin.Begin);
        }

        await using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        if (position != null)
        {
            stream.Seek(position.Value, SeekOrigin.Begin);
        }

        return memoryStream.ToArray();
    }
}
