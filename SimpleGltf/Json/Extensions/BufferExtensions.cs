using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleGltf.Json.Extensions
{
    public static class BufferExtensions
    {
        public static IEnumerable<BufferView> GetBufferViews(this Buffer buffer)
        {
            return buffer.GltfAsset.BufferViews.Where(bufferView => bufferView.Buffer == buffer);
        }

        public static async Task<Stream> GetStreamAsync(this Buffer buffer)
        {
            var stream = new MemoryStream();
            foreach (var bufferView in buffer.GetBufferViews())
            {
                stream.Seek(bufferView.ByteOffset ?? 0, SeekOrigin.Begin);
                await using var bufferViewStream = await bufferView.GetStreamAsync();
                await bufferViewStream.CopyToAsync(stream);
            }

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}