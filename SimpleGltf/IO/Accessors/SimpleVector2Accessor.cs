using SimpleGltf.Converters;
using SimpleGltf.Json;
using SimpleGltf.Json.Enums;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.IO.Accessors
{
    public class SimpleVector2Accessor<T> : SimpleAccessor
    {
        internal SimpleVector2Accessor(BufferView bufferView, bool minMax = false, bool? normalized = null) : base(
            bufferView,
            ComponentTypeConverter.Convert(typeof(T)), AccessorType.Vec2, minMax, normalized)
        {
            var componentSize = ComponentTypeConverter.GetSize(ComponentType);
        }

        public void Write(T x, T y)
        {
            EnsureOffset();
            Accessor.Write((dynamic) x);
            Accessor.Write((dynamic) y);

            Accessor.NextComponent();
        }
    }
}