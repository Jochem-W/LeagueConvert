using SimpleGltf.Converters;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;
using SimpleGltf.Json;

namespace SimpleGltf.IO.Accessors
{
    public class SimpleScalarAccessor<T> : SimpleAccessor
    {
        internal SimpleScalarAccessor(BufferView bufferView, bool minMax = false, bool? normalized = null) : base(
            bufferView,
            AccessorComponentTypeConverter.Convert(typeof(T)), AccessorType.Scalar, minMax, normalized)
        {
            var componentSize = AccessorComponentTypeConverter.GetSize(AccessorComponentType);
            Size = componentSize;
            Size = Size.Offset();
        }

        public void Write(T value)
        {
            Accessor.Write((dynamic) value);

            Accessor.Count++;
            Accessor.NextComponent();
        }
    }
}