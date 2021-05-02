using System.Collections.Generic;

namespace SimpleGltf.Extensions
{
    public static class IListExtensions
    {
        public static int? NullableIndexOf<T>(this IList<T> list, T obj)
        {
            var value = list.IndexOf(obj);
            if (value != -1)
                return value;
            return null;
        }
    }
}