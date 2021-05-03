using System.IO;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;
using SimpleGltf.Json;

namespace SimpleGltf.IO.Accessors
{
    public class SimpleAccessor
    {
        internal readonly Accessor Accessor;
        internal readonly ComponentType ComponentType;

        internal SimpleAccessor(BufferView bufferView, ComponentType componentType,
            AccessorType accessorType, bool minMax, bool? normalized)
        {
            Accessor = new Accessor(bufferView, componentType, accessorType, minMax, normalized);
            ComponentType = componentType;
        }

        internal int Size
        {
            get => Accessor.Size;
            init => Accessor.Size = value;
        }

        internal void EnsureOffset()
        {
            Accessor.BufferView.BinaryWriter.Seek(
                (int) Accessor.BufferView.BinaryWriter.BaseStream.Length.GetOffset(), SeekOrigin.Current);
        }
    }
}