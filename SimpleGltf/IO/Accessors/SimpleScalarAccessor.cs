using SimpleGltf.Converters;
using SimpleGltf.Enums;
using SimpleGltf.Json;

namespace SimpleGltf.IO.Accessors
{
    public class SimpleScalarAccessor<T> : SimpleAccessor
    {
        internal SimpleScalarAccessor(BufferView bufferView, bool minMax = false, bool? normalized = null) : base(
            bufferView,
            ComponentTypeConverter.Convert(typeof(T)), AccessorType.Scalar, minMax, normalized)
        {
            var componentSize = ComponentTypeConverter.GetSize(ComponentType);
        }

        public void Write(T value)
        {
            Accessor.Write((dynamic) value);

            Accessor.NextComponent();
        }
    }
}