using SimpleGltf.Enums;

namespace SimpleGltf.Json.Extensions
{
    public static class BufferViewExtensions
    {
        public static SByteAccessor CreateSByteAccessor(this BufferView bufferView, AccessorType accessorType,
            bool minMax = false, bool normalized = false, string name = null)
        {
            return new(bufferView, accessorType, minMax, normalized);
        }

        public static ByteAccessor CreateByteAccessor(this BufferView bufferView, AccessorType accessorType,
            bool minMax = false, bool normalized = false, string name = null)
        {
            return new(bufferView, accessorType, minMax, normalized);
        }

        public static ShortAccessor CreateShortAccessor(this BufferView bufferView, AccessorType accessorType,
            bool minMax = false, bool normalized = false, string name = null)
        {
            return new(bufferView, accessorType, minMax, normalized);
        }

        public static UShortAccessor CreateUShortAccessor(this BufferView bufferView, AccessorType accessorType,
            bool minMax = false, bool normalized = false, string name = null)
        {
            return new(bufferView, accessorType, minMax, normalized);
        }

        public static UIntAccessor CreateUIntAccessor(this BufferView bufferView, AccessorType accessorType,
            bool minMax = false, bool normalized = false, string name = null)
        {
            return new(bufferView, accessorType, minMax, normalized);
        }

        public static FloatAccessor CreateFloatAccessor(this BufferView bufferView, AccessorType accessorType,
            bool minMax = false, bool normalized = false, string name = null)
        {
            return new(bufferView, accessorType, minMax, normalized);
        }
    }
}