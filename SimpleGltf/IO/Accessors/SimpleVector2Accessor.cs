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
            AccessorComponentTypeConverter.Convert(typeof(T)), AccessorType.Vector2, minMax, normalized)
        {
            var componentSize = AccessorComponentTypeConverter.GetSize(AccessorComponentType);
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