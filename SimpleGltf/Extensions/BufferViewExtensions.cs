using System.Collections.Generic;
using System.Linq;
using SimpleGltf.Json;

namespace SimpleGltf.Extensions
{
    internal static class BufferViewExtensions
    {
        internal static IEnumerable<Accessor> GetAccessors(this BufferView bufferView)
        {
            return bufferView.Buffer.GltfAsset.Accessors.Where(accessor => accessor.BufferView == bufferView);
        }

        internal static int? GetByteOffset(this BufferView bufferView)
        {
            var take = true;
            var byteOffset = bufferView.Buffer.GetBufferViews().TakeWhile(b =>
            {
                if (b == bufferView)
                    take = false;
                return take;
            }).GetLength();
            if (byteOffset == 0)
                return null;
            return byteOffset.Offset();
        }

        internal static int GetLength(this IEnumerable<BufferView> bufferViews)
        {
            var length = 0;
            foreach (var bufferView in bufferViews)
            {
                length += length.GetOffset();
                length += bufferView.ByteLength;
            }

            return length;
        }
    }
}