using System;
using SimpleGltf.Enums;

namespace SimpleGltf.Extensions
{
    internal static class ComponentTypeExtensions
    {
        internal static int GetElementSize(this ComponentType componentType)
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
    }
}