using System;
using SimpleGltf.Enums;

namespace SimpleGltf.Extensions
{
    internal static class ComponentTypeExtensions
    {
        internal static int GetSize(this ComponentType componentType)
        {
            return componentType switch
            {
                ComponentType.SByte => sizeof(sbyte),
                ComponentType.Byte => sizeof(byte),
                ComponentType.Short => sizeof(short),
                ComponentType.UShort => sizeof(ushort),
                ComponentType.UInt => sizeof(uint),
                ComponentType.Float => sizeof(float),
                _ => throw new ArgumentOutOfRangeException(nameof(componentType), componentType, null)
            };
        }

        internal static ComponentType GetComponentType(this Type type)
        {
            return Type.GetTypeCode(type) switch
            {
                TypeCode.SByte => ComponentType.SByte,
                TypeCode.Byte => ComponentType.Byte,
                TypeCode.Int16 => ComponentType.Short,
                TypeCode.UInt16 => ComponentType.UShort,
                TypeCode.UInt32 => ComponentType.UInt,
                TypeCode.Single => ComponentType.Float,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}