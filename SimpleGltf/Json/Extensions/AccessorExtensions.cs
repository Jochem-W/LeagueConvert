using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleGltf.Extensions;
using SimpleGltf.Helpers;

namespace SimpleGltf.Json.Extensions
{
    public static class AccessorExtensions
    {
        public static void Write<T>(this Accessor<T> accessor, params T[] components) where T : struct, IComparable
        {
            accessor.Values.AddRange(components.Cast<dynamic>());
            accessor.Count++;
        }

        internal static void WriteToBinaryWriter(this Accessor accessor, BinaryWriter binaryWriter, int index)
        {
            var offset = index * accessor.ComponentCount;
            for (var i = 0; i < accessor.ComponentCount; i++)
                binaryWriter.Write(accessor.Values[i + offset]);
        }

        internal static StrideHelper GetStride(this IEnumerable<Accessor> accessors, bool vertexAttributes)
        {
            var stride = new StrideHelper();
            foreach (var accessor in accessors)
                stride.Lengths.Add(accessor.ElementSize + accessor.ElementSize.GetOffset());
            stride.Total = stride.Lengths.Sum();
            var offset = 0;
            foreach (var length in stride.Lengths)
            {
                stride.Offsets.Add(offset);
                offset += length;
            }
            return stride;
        }
    }
}