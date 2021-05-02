using SimpleGltf.Converters;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;
using SimpleGltf.Json;

namespace SimpleGltf.IO.Accessors
{
    public class SimpleVector4Accessor<T> : SimpleAccessor
    {
        internal SimpleVector4Accessor(BufferView bufferView, bool minMax = false, bool? normalized = null) : base(
            bufferView,
            AccessorComponentTypeConverter.Convert(typeof(T)), AccessorType.Vector4, minMax, normalized)
        {
            var componentSize = AccessorComponentTypeConverter.GetSize(AccessorComponentType);
            Size = 4 * componentSize;
            Size = Size.Offset();
        }

        public void Write(T x, T y, T z, T w)
        {
            EnsureOffset();
            Accessor.Write((dynamic) x);
            Accessor.Write((dynamic) y);
            Accessor.Write((dynamic) z);
            Accessor.Write((dynamic) w);

            Accessor.Count++;
            Accessor.NextComponent();
        }
    }
}