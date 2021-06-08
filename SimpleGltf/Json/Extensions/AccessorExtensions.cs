using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json.Extensions
{
    public static class AccessorExtensions
    {
        public static void Write<T>(this Accessor<T> accessor, params T[] components) where T : struct, IComparable
        {
            if (accessor.Type != AccessorType.Mat2 &&
                accessor.Type != AccessorType.Mat3 &&
                accessor.Type != AccessorType.Mat4)
            {
                accessor.Values.AddRange(components.Cast<dynamic>());
                accessor.Count++;
                return;
            }

            var rows = accessor.Type.GetRows();
            var columns = accessor.Type.GetColumns();
            for (var i = 0; i < columns; i++)
            for (var j = 0; j < rows; j++)
                accessor.Values.Add(components[rows * j + i]);
            accessor.Count++;
        }

        internal static void WriteToBinaryWriter(this Accessor accessor, BinaryWriter binaryWriter, int index)
        {
            var offset = index * accessor.ComponentCount;
            for (var i = 0; i < accessor.ComponentCount; i++)
                binaryWriter.Write(accessor.Values[i + offset]);
        }

        internal static void CalculateOffset(this Accessor accessor)
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

        internal static void CalculateStride(this IEnumerable<Accessor> accessors)
        {
            var accessorList = accessors.ToList();
            if (accessorList.Select(accessor => accessor.BufferView).Distinct().Count() > 1)
                throw new NotImplementedException(); // multiple bufferViews
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