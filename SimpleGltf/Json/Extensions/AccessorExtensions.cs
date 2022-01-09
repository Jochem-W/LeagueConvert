using SimpleGltf.Extensions;

namespace SimpleGltf.Json.Extensions;

public static class AccessorExtensions
{
    public static Accessor SetOffset(this Accessor accessor, int count)
    {
        accessor.ActualByteOffset = count * accessor.ComponentType.GetSize();
        return accessor;
    }
}