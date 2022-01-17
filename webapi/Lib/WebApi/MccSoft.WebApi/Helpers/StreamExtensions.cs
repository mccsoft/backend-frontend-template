using System.IO;

namespace MccSoft.WebApi.Helpers
{
    public static class StreamExtensions
    {
        public static byte[] ToArray(this Stream stream)
        {
            long? position = null;
            if (stream.CanSeek)
            {
                position = stream.Position;
                stream.Seek(0, SeekOrigin.Begin);
            }
            
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            
            if (position != null)
            {
                stream.Seek(position.Value, SeekOrigin.Begin);
            }
            
            return memoryStream.ToArray();
        }
    }
}