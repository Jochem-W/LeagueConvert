using System;
using SimpleGltf.Enums;

namespace SimpleGltf.Converters
{
    internal static class AccessorComponentTypeConverter
    {
        internal static AccessorComponentType Convert(Type type)
        {
            return Type.GetTypeCode(type) switch
            {
                TypeCode.SByte => AccessorComponentType.SByte,
                TypeCode.Byte => AccessorComponentType.Byte,
                TypeCode.Int16 => AccessorComponentType.Short,
                TypeCode.UInt16 => AccessorComponentType.UShort,
                TypeCode.UInt32 => AccessorComponentType.UInt,
                TypeCode.Single => AccessorComponentType.Float,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        internal static int GetSize(AccessorComponentType accessorComponentType)
        {
            return accessorComponentType switch
            {
                AccessorComponentType.SByte => 1,
                AccessorComponentType.Byte => 1,
                AccessorComponentType.Short => 2,
                AccessorComponentType.UShort => 2,
                AccessorComponentType.UInt => 4,
                AccessorComponentType.Float => 4,
                _ => throw new ArgumentOutOfRangeException(nameof(accessorComponentType), accessorComponentType, null)
            };
        }
    }
}