using SimpleGltf.Converters;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;
using SimpleGltf.Json;

namespace SimpleGltf.IO.Accessors
{
    public class SimpleVector3Accessor<T> : SimpleAccessor
    {
        internal SimpleVector3Accessor(BufferView bufferView, bool minMax = false, bool? normalized = null) : base(
            bufferView,
            ComponentTypeConverter.Convert(typeof(T)), AccessorType.Vec3, minMax, normalized)
        {
            var componentSize = ComponentTypeConverter.GetSize(ComponentType);
            Size = 3 * componentSize;
            Size = Size.Offset();
        }

        public void Write(T x, T y, T z)
        {
            EnsureOffset();
            Accessor.Write((dynamic) x);
            Accessor.Write((dynamic) y);
            Accessor.Write((dynamic) z);

            Accessor.Count++;
            Accessor.NextComponent();
        }
    }
}