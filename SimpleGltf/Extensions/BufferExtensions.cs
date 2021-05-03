using System.Collections.Generic;
using System.Linq;
using SimpleGltf.Json;

namespace SimpleGltf.Extensions
{
    internal static class BufferExtensions
    {
        internal static IEnumerable<BufferView> GetBufferViews(this Buffer buffer)
        {
            return buffer.GltfAsset.BufferViews.Where(bufferView => bufferView.Buffer == buffer);
        }
    }
}