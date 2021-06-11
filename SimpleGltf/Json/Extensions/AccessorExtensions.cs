using System;
using System.Collections.Generic;
using System.Linq;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json.Extensions
{
    public static class AccessorExtensions
    {
        internal static void CalculateOffset(this IAccessor accessor)
        {
            int offset;
            if (accessor.BufferView.Target == BufferViewTarget.ArrayBuffer ||
                accessor.Type is AccessorType.Mat2 or AccessorType.Mat3 or AccessorType.Mat4)
                offset = accessor.BufferView.ByteOffset.GetValueOrDefault().GetOffset(4);
            else
                offset = accessor.BufferView.ByteOffset.GetValueOrDefault().GetOffset(accessor.ComponentTypeLength);
            if (offset == 0)
                return;
            accessor.BufferView.ByteOffset += offset;
        }

        internal static void CalculateStride(this IEnumerable<IAccessor> accessors)
        {
            var accessorList = accessors.ToList();
            if (accessorList.Select(accessor => accessor.BufferView).Distinct().Count() > 1)
                throw new ArgumentException("Supplied accessors need to use the same bufferView", nameof(accessors));
            var bytesBefore = 0;
            foreach (var accessor in accessorList)
            {
                var offset = bytesBefore;
                if (accessor.BufferView.Target == BufferViewTarget.ArrayBuffer ||
                    accessor.Type is AccessorType.Mat2 or AccessorType.Mat3 or AccessorType.Mat4)
                    offset += bytesBefore.GetOffset(4);
                accessor.ByteOffset = offset;
                bytesBefore = offset + accessor.ElementSize;
            }

            accessorList[0].BufferView.ByteStride = bytesBefore;
        }
    }
}