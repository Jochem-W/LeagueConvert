using System;
using System.Collections.Generic;
using System.IO;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json
{
    public class SByteAccessor : Accessor
    {
        private readonly int _columns;
        private readonly int _componentCount;
        private readonly sbyte[] _max;
        private readonly sbyte[] _min;
        private readonly bool _minMax;
        private readonly int _rows;
        private bool _firstElement;

        internal SByteAccessor(BufferView bufferView, AccessorType type, bool minMax, bool normalized) :
            base(bufferView, type, normalized)
        {
            ComponentType = ComponentType.SByte;
            _componentCount = Type.GetColumns() * Type.GetRows();
            _minMax = minMax;
            _rows = Type.GetRows();
            _columns = Type.GetColumns();
            _firstElement = true;
            if (!_minMax)
                return;
            _min = new sbyte[_componentCount];
            _max = new sbyte[_componentCount];
            for (var i = 0; i < _componentCount; i++)
            {
                _min[i] = sbyte.MaxValue;
                _max[i] = sbyte.MinValue;
            }
        }

        public void Write(params sbyte[] components)
        {
            if (BufferView.Target == BufferViewTarget.ArrayBuffer)
                BufferView.BinaryWriter.Seek((int) BufferView.BinaryWriter.BaseStream.Position.GetOffset(4),
                    SeekOrigin.Current);
            if (_firstElement)
                ActualByteOffset = (int) BufferView.BinaryWriter.BaseStream.Position;
            var element = new List<sbyte>(_componentCount);

            switch (Type)
            {
                case AccessorType.Scalar:
                case AccessorType.Vec2:
                case AccessorType.Vec3:
                case AccessorType.Vec4:
                    foreach (var component in components)
                    {
                        BufferView.BinaryWriter.Write(component);
                        element.Add(component);
                    }

                    break;
                case AccessorType.Mat2:
                case AccessorType.Mat3:
                case AccessorType.Mat4:
                    for (var i = 0; i < _columns; i++)
                    {
                        BufferView.BinaryWriter.Seek((int) BufferView.BinaryWriter.BaseStream.Position.GetOffset(4),
                            SeekOrigin.Current);
                        for (var j = 0; j < _rows; j++)
                        {
                            var component = components[_rows * j + i];
                            BufferView.BinaryWriter.Write(component);
                            element.Add(component);
                        }
                    }

                    break;
                default:
                    throw new NotImplementedException();
            }

            Count++;
            if (_firstElement)
            {
                if (BufferView.Stride)
                    BufferView.ActualByteStride = (int) BufferView.BinaryWriter.BaseStream.Position;
                _firstElement = false;
            }

            if (!_minMax)
                return;
            for (var i = 0; i < element.Count; i++)
            {
                if (element[i] < _min[i])
                    _min[i] = element[i];
                if (element[i] > _max[i])
                    _max[i] = element[i];
            }

            Max = _max;
            Min = _min;
        }
    }
}