using System;
using SimpleGltf.Enums;
using SimpleGltf.Json.Enums;

namespace SimpleGltf.Converters
{
    internal static class ComponentTypeConverter
    {
        internal static ComponentType Convert(Type type)
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

        internal static int GetSize(ComponentType componentType)
        {
            return componentType switch
            {
                ComponentType.SByte => 1,
                ComponentType.Byte => 1,
                ComponentType.Short => 2,
                ComponentType.UShort => 2,
                ComponentType.UInt => 4,
                ComponentType.Float => 4,
                _ => throw new ArgumentOutOfRangeException(nameof(componentType), componentType, null)
            };
        }
    }
}