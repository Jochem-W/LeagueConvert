using System;
using System.Collections.Generic;
using System.Linq;
using SimpleGltf.Enums;

namespace SimpleGltf.Json.Extensions
{
    public static class BufferViewExtensions
    {
        public static Accessor<T> CreateAccessor<T>(this BufferView bufferView, AccessorType accessorType,
            bool minMax = false, bool normalized = false, string name = null) where T : struct, IComparable
        {
            return new(bufferView, accessorType, minMax, normalized, name);
        }

        public static IEnumerable<Accessor> GetAccessors(this BufferView bufferView)
        {
            return bufferView.GltfAsset.Accessors.Where(accessor => accessor.BufferView == bufferView);
        }
    }
}