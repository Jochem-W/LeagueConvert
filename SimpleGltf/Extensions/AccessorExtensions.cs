using System.Collections.Generic;
using System.Linq;
using SimpleGltf.Json;

namespace SimpleGltf.Extensions
{
    internal static class AccessorExtensions
    {
        internal static int GetStride(this IEnumerable<Accessor> accessors)
        {
            return accessors.Select(accessor => accessor.Size).Sum();
        }
    }
}