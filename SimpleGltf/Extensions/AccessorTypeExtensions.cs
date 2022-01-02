using SimpleGltf.Enums;

namespace SimpleGltf.Extensions;

internal static class AccessorTypeExtensions
{
    internal static int GetColumns(this AccessorType accessorType)
    {
        return accessorType switch
        {
            AccessorType.Scalar => 1,
            AccessorType.Vec2 => 1,
            AccessorType.Vec3 => 1,
            AccessorType.Vec4 => 1,
            AccessorType.Mat2 => 2,
            AccessorType.Mat3 => 3,
            AccessorType.Mat4 => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(accessorType), accessorType, null)
        };
    }

    internal static int GetRows(this AccessorType accessorType)
    {
        return accessorType switch
        {
            AccessorType.Scalar => GetColumns(accessorType),
            AccessorType.Vec2 => 2,
            AccessorType.Vec3 => 3,
            AccessorType.Vec4 => 4,
            AccessorType.Mat2 => GetColumns(accessorType),
            AccessorType.Mat3 => GetColumns(accessorType),
            AccessorType.Mat4 => GetColumns(accessorType),
            _ => throw new ArgumentOutOfRangeException(nameof(accessorType), accessorType, null)
        };
    }
}