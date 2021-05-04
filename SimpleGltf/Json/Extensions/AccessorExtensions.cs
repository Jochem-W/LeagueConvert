using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleGltf.Extensions;
using SimpleGltf.Json.Enums.Extensions;

namespace SimpleGltf.Json.Extensions
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

        internal static void SetSize(this Accessor accessor)
        {
            var elementSize = accessor.ComponentType.GetElementSize();
            var rows = accessor.Type.GetRows();
            var columns = accessor.Type.GetColumns();
            for (var i = 0; i < columns; i++)
            {
                accessor.ComponentSize += accessor.ComponentSize.GetOffset();
                accessor.ComponentSize += rows * elementSize;
            }
        }

        internal static void NextComponent(this Accessor accessor)
        {
            accessor.Count++;
            if (!accessor.MinMax)
            {
                accessor.Component.Clear();
                return;
            }

            if (accessor.Component.Count == 1)
            {
                if (accessor.Min == null || accessor.Max == null)
                {
                    accessor.Min = accessor.Component[0];
                    accessor.Max = accessor.Component[0];
                    accessor.Component.Clear();
                    return;
                }

                if (accessor.Component[0] < accessor.Min)
                    accessor.Min = accessor.Component[0];
                if (accessor.Component[0] > accessor.Max)
                    accessor.Max = accessor.Component[0];
                accessor.Component.Clear();
                return;
            }

            if (accessor.Min == null && accessor.Max == null)
            {
                accessor.Min = new List<dynamic>(accessor.Component);
                accessor.Max = new List<dynamic>(accessor.Component);
                accessor.Component.Clear();
                return;
            }

            for (var i = 0; i < accessor.Component.Count; i++)
            {
                if (accessor.Component[i] < accessor.Min[i])
                    accessor.Min[i] = accessor.Component[i];
                if (accessor.Component[i] > accessor.Max[i])
                    accessor.Max[i] = accessor.Component[i];
            }

            accessor.Component.Clear();
        }
    }
}