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

        internal void EnsureOffset()
        {
            Accessor.BinaryWriter.Seek(
                (int) Accessor.BinaryWriter.BaseStream.Length.GetOffset(), SeekOrigin.Current);
        }
    }
}