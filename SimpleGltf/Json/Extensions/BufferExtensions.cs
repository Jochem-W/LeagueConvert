using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SimpleGltf.Enums;
using SimpleGltf.Json.Enums;

namespace SimpleGltf.Json.Extensions
{
    internal static class BufferExtensions
    {
        internal static IEnumerable<BufferView> GetBufferViews(this Buffer buffer)
        {
            return buffer.GltfAsset.BufferViews.Where(bufferView => bufferView.Buffer == buffer);
        }

        internal static async Task<Stream> GetStreamAsync(this Buffer buffer)
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

        internal static BufferView CreateBufferView(this Buffer buffer, BufferViewTarget target, string name = null)
        {
            return new(buffer, target, name);
        }
    }
}