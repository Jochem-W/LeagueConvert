using SimpleGltf.Converters;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;
using SimpleGltf.Json;

namespace SimpleGltf.IO.Accessors
{
    public class SimpleVector2Accessor<T> : SimpleAccessor
    {
        internal SimpleVector2Accessor(BufferView bufferView, bool minMax = false, bool? normalized = null) : base(
            bufferView,
            ComponentTypeConverter.Convert(typeof(T)), AccessorType.Vec2, minMax, normalized)
        {
            var componentSize = ComponentTypeConverter.GetSize(ComponentType);
            Size = 2 * componentSize;
            Size = Size.Offset();
        }

        public void Write(T x, T y)
        {
            EnsureOffset();
            Accessor.Write((dynamic) x);
            Accessor.Write((dynamic) y);

            Accessor.Count++;
            Accessor.NextComponent();
        }
    }
}