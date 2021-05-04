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
        public static Accessor SetBufferView(this Accessor accessor, BufferView bufferView)
        {
            accessor.BufferView = bufferView;
            return accessor;
        }
        
        public static int GetStride(this IEnumerable<Accessor> accessors)
        {
            return accessors.Select(accessor => accessor.ComponentSize).Sum();
        }

        public static long GetLength(this IEnumerable<Accessor> accessors)
        {
            return accessors.Sum(accessor => accessor.ByteLength);
        }

        public static void SeekToBegin(this IEnumerable<Accessor> accessors)
        {
            foreach (var accessor in accessors)
                accessor.BinaryWriter.Seek(0, SeekOrigin.Begin);
        }

        public static void SetSize(this Accessor accessor)
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

        public static void NextComponent(this Accessor accessor)
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

        public static void WriteComponent(this Accessor accessor, params dynamic[] values)
        {
            switch (accessor.Type)
            {
                case AccessorType.Scalar:
                    WriteElement(accessor, values[0]);
                    accessor.NextComponent();
                    break;
                case AccessorType.Vec2:
                    accessor.EnsureOffset();
                    WriteElement(accessor, values[0]);
                    WriteElement(accessor, values[1]);
                    accessor.NextComponent();
                    break;
                case AccessorType.Vec3:
                    accessor.EnsureOffset();
                    WriteElement(accessor, values[0]);
                    WriteElement(accessor, values[1]);
                    WriteElement(accessor, values[2]);
                    accessor.NextComponent();
                    break;
                case AccessorType.Vec4:
                    accessor.EnsureOffset();
                    WriteElement(accessor, values[0]);
                    WriteElement(accessor, values[1]);
                    WriteElement(accessor, values[2]);
                    WriteElement(accessor, values[3]);
                    accessor.NextComponent();
                    break;
                case AccessorType.Mat2:
                    accessor.EnsureOffset();
                    WriteElement(accessor, values[0]);
                    WriteElement(accessor, values[2]);
                    accessor.EnsureOffset();
                    WriteElement(accessor, values[1]);
                    WriteElement(accessor, values[3]);
                    accessor.NextComponent();
                    break;
                case AccessorType.Mat3:
                    accessor.EnsureOffset();
                    WriteElement(accessor, values[0]);
                    WriteElement(accessor, values[3]);
                    WriteElement(accessor, values[6]);
                    accessor.EnsureOffset();
                    WriteElement(accessor, values[1]);
                    WriteElement(accessor, values[4]);
                    WriteElement(accessor, values[7]);
                    accessor.EnsureOffset();
                    WriteElement(accessor, values[2]);
                    WriteElement(accessor, values[5]);
                    WriteElement(accessor, values[8]);
                    accessor.NextComponent();
                    break;
                case AccessorType.Mat4:
                    accessor.EnsureOffset();
                    WriteElement(accessor, values[0]);
                    WriteElement(accessor, values[4]);
                    WriteElement(accessor, values[8]);
                    WriteElement(accessor, values[12]);
                    accessor.EnsureOffset();
                    WriteElement(accessor, values[1]);
                    WriteElement(accessor, values[5]);
                    WriteElement(accessor, values[9]);
                    WriteElement(accessor, values[13]);
                    accessor.EnsureOffset();
                    WriteElement(accessor, values[2]);
                    WriteElement(accessor, values[6]);
                    WriteElement(accessor, values[10]);
                    WriteElement(accessor, values[14]);
                    accessor.EnsureOffset();
                    WriteElement(accessor, values[3]);
                    WriteElement(accessor, values[7]);
                    WriteElement(accessor, values[11]);
                    WriteElement(accessor, values[15]);
                    accessor.NextComponent();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void WriteElement(Accessor accessor, dynamic value)
        {
            switch (accessor.ComponentType)
            {
                case ComponentType.Byte when value is byte:
                case ComponentType.SByte when value is sbyte:
                case ComponentType.Short when value is short:
                case ComponentType.UShort when value is ushort:
                case ComponentType.UInt when value is uint:
                case ComponentType.Float when value is float:
                    accessor.Write(value);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void EnsureOffset(this Accessor accessor)
        {
            accessor.BinaryWriter.Seek((int) accessor.BinaryWriter.BaseStream.Length.GetOffset(), SeekOrigin.Current);
        }
    }
}