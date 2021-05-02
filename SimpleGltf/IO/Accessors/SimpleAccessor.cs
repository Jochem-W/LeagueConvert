using System.IO;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;
using SimpleGltf.Json;

namespace SimpleGltf.IO.Accessors
{
    public class SimpleAccessor
    {
        internal readonly Accessor Accessor;
        internal readonly AccessorComponentType AccessorComponentType;

        internal SimpleAccessor(BufferView bufferView, AccessorComponentType accessorComponentType,
            AccessorType accessorType, bool minMax, bool? normalized)
        {
            Accessor = new Accessor(bufferView, accessorComponentType, accessorType, minMax, normalized);
            AccessorComponentType = accessorComponentType;
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