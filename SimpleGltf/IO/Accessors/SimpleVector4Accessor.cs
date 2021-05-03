using SimpleGltf.Converters;
using SimpleGltf.Enums;
using SimpleGltf.Json;

namespace SimpleGltf.IO.Accessors
{
    public class SimpleVector4Accessor<T> : SimpleAccessor
    {
        internal SimpleVector4Accessor(BufferView bufferView, bool minMax = false, bool? normalized = null) : base(
            bufferView,
            ComponentTypeConverter.Convert(typeof(T)), AccessorType.Vec4, minMax, normalized)
        {
            var componentSize = ComponentTypeConverter.GetSize(ComponentType);
        }

        public void Write(T x, T y, T z, T w)
        {
            EnsureOffset();
            Accessor.Write((dynamic) x);
            Accessor.Write((dynamic) y);
            Accessor.Write((dynamic) z);
            Accessor.Write((dynamic) w);

            Accessor.NextComponent();
        }
    }
}