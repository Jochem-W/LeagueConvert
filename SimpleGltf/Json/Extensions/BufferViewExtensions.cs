using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json.Extensions
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

        internal static async Task<Stream> GetStreamAsync(this BufferView bufferView)
        {
            var stream = new MemoryStream();
            var stride = bufferView.ByteStride;
            var accessors = bufferView.GetAccessors().ToList();
            accessors.SeekToBegin();
            if (stride == null)
            {
                foreach (var accessor in accessors)
                    await accessor.BinaryWriter.BaseStream.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }

            var totalLength = accessors.GetLength();
            while (stream.Length != totalLength)
                foreach (var accessor in accessors)
                {
                    var memory = new byte[accessor.ComponentSize];
                    await accessor.BinaryWriter.BaseStream.ReadAsync(memory);
                    await stream.WriteAsync(memory);
                }

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        internal static Accessor CreateAccessor(this BufferView bufferView, ComponentType componentType,
            AccessorType accessorType, bool? normalized = null, string name = null, bool minMax = false)
        {
            return new(bufferView, componentType, accessorType, normalized, name, minMax);
        }
    }
}