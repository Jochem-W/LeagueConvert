using SimpleGltf.Converters;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;
using SimpleGltf.Json;

namespace SimpleGltf.IO.Accessors
{
    public class SimpleMatrix4x4Accessor<T> : SimpleAccessor
    {
        internal SimpleMatrix4x4Accessor(BufferView bufferView, bool minMax = false, bool? normalized = null) : base(
            bufferView,
            AccessorComponentTypeConverter.Convert(typeof(T)), AccessorType.Matrix4x4, minMax, normalized)
        {
            var componentSize = AccessorComponentTypeConverter.GetSize(AccessorComponentType);
            Size = 4 * componentSize;
            Size = Size.Offset();
            Size += 4 * componentSize;
            Size = Size.Offset();
            Size += 4 * componentSize;
            Size = Size.Offset();
            Size += 4 * componentSize;
            Size = Size.Offset();
        }

        public void Write(
            T m11, T m12, T m13, T m14,
            T m21, T m22, T m23, T m24,
            T m31, T m32, T m33, T m34,
            T m41, T m42, T m43, T m44)
        {
            EnsureOffset();
            Accessor.Write((dynamic) m11);
            Accessor.Write((dynamic) m21);
            Accessor.Write((dynamic) m31);
            Accessor.Write((dynamic) m41);

            EnsureOffset();
            Accessor.Write((dynamic) m12);
            Accessor.Write((dynamic) m22);
            Accessor.Write((dynamic) m32);
            Accessor.Write((dynamic) m42);

            EnsureOffset();
            Accessor.Write((dynamic) m13);
            Accessor.Write((dynamic) m23);
            Accessor.Write((dynamic) m33);
            Accessor.Write((dynamic) m43);

            EnsureOffset();
            Accessor.Write((dynamic) m14);
            Accessor.Write((dynamic) m24);
            Accessor.Write((dynamic) m34);
            Accessor.Write((dynamic) m44);

            Accessor.Count++;
            Accessor.NextComponent();
        }
    }
}