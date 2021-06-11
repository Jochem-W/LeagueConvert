using System.Collections.Generic;
using System.Linq;
using SimpleGltf.Enums;

namespace SimpleGltf.Json.Extensions
{
    public static class BufferExtensions
    {
        public static BufferView CreateBufferView(this Buffer buffer, BufferViewTarget? target = null,
            string name = null)
        {
            return new(buffer, target, name);
        }

        internal static IEnumerable<BufferView> GetBufferViews(this Buffer buffer)
        {
            return buffer.GltfAsset.BufferViews.Where(bufferView => bufferView.Buffer == buffer);
        }
    }
}