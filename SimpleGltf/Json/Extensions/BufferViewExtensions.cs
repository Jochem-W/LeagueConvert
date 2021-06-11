using System.Collections.Generic;
using System.Linq;
using SimpleGltf.Enums;

namespace SimpleGltf.Json.Extensions
{
    public static class BufferViewExtensions
    {
        public static SByteAccessor CreateSByteAccessor(this BufferView bufferView, AccessorType accessorType,
            bool minMax = false, bool normalized = false, string name = null)
        {
            return new(bufferView, accessorType, minMax, normalized, name);
        }

        public static ByteAccessor CreateByteAccessor(this BufferView bufferView, AccessorType accessorType,
            bool minMax = false, bool normalized = false, string name = null)
        {
            return new(bufferView, accessorType, minMax, normalized, name);
        }

        public static ShortAccessor CreateShortAccessor(this BufferView bufferView, AccessorType accessorType,
            bool minMax = false, bool normalized = false, string name = null)
        {
            return new(bufferView, accessorType, minMax, normalized, name);
        }

        public static UShortAccessor CreateUShortAccessor(this BufferView bufferView, AccessorType accessorType,
            bool minMax = false, bool normalized = false, string name = null)
        {
            return new(bufferView, accessorType, minMax, normalized, name);
        }

        public static UIntAccessor CreateUIntAccessor(this BufferView bufferView, AccessorType accessorType,
            bool minMax = false, bool normalized = false, string name = null)
        {
            return new(bufferView, accessorType, minMax, normalized, name);
        }

        public static FloatAccessor CreateFloatAccessor(this BufferView bufferView, AccessorType accessorType,
            bool minMax = false, bool normalized = false, string name = null)
        {
            return new(bufferView, accessorType, minMax, normalized, name);
        }


        public static IEnumerable<IAccessor> GetAccessors(this BufferView bufferView)
        {
            return bufferView.GltfAsset.Accessors.Where(accessor => accessor.BufferView == bufferView);
        }
    }
}