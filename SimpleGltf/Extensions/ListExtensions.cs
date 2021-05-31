using System.Collections.Generic;
using System.Linq;

namespace SimpleGltf.Extensions
{
    internal static class ListExtensions
    {
        internal static IEnumerable<List<T>> Batch<T>(this IList<T> list, int size)
        {
            for (var i = 0; i < list.Count; i += size)
                yield return list.Skip(i).Take(size).ToList();
        }
    }
}