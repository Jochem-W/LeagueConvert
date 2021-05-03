using SimpleGltf.Converters;
using SimpleGltf.Enums;
using SimpleGltf.Json;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.IO.Accessors
{
    public class SimpleMatrix2x2Accessor<T> : SimpleAccessor
    {
        internal SimpleMatrix2x2Accessor(BufferView bufferView, bool minMax = false, bool? normalized = null) : base(
            bufferView,
            ComponentTypeConverter.Convert(typeof(T)), AccessorType.Mat2, minMax, normalized)
        {
            var componentSize = ComponentTypeConverter.GetSize(ComponentType);
        }

        public void Write(T m11, T m12, T m21, T m22)
        {
            EnsureOffset();
            Accessor.Write((dynamic) m11);
            Accessor.Write((dynamic) m21);

            EnsureOffset();
            Accessor.Write((dynamic) m12);
            Accessor.Write((dynamic) m22);

            Accessor.NextComponent();
        }
    }
}