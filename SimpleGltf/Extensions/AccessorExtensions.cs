using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleGltf.Json;

namespace SimpleGltf.Extensions
{
    internal static class AccessorExtensions
    {
        internal static int GetStride(this IEnumerable<Accessor> accessors)
        {
            return accessors.Select(accessor => accessor.ComponentSize).Sum();
        }

        internal static long GetLength(this IEnumerable<Accessor> accessors)
        {
            return accessors.Sum(accessor => accessor.ByteLength);
        }

        internal static void SeekToBegin(this IEnumerable<Accessor> accessors)
        {
            foreach (var accessor in accessors)
                accessor.BinaryWriter.Seek(0, SeekOrigin.Begin);
        }
    }
}