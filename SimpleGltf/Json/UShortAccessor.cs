using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json
{
    public class UShortAccessor : IAccessor
    {
        private readonly int _columns;
        private readonly int _componentCount;
        private readonly ushort[] _max;
        private readonly ushort[] _min;
        private readonly bool _minMax;
        private readonly int _rows;
        private int _actualByteOffset;
        private bool _firstElement;

        internal UShortAccessor(BufferView bufferView, AccessorType type, bool minMax, bool normalized, string name)
        {
            BufferView = bufferView;
            GltfAsset = BufferView.GltfAsset;
            GltfAsset.Accessors ??= new List<IAccessor>();
            Index = GltfAsset.Accessors.Count;
            GltfAsset.Accessors.Add(this);
            ComponentType = ComponentType.UShort;
            Type = type;
            if (normalized)
                Normalized = true;
            Name = name;
            _componentCount = Type.GetColumns() * Type.GetRows();
            _minMax = minMax;
            _rows = Type.GetRows();
            _columns = Type.GetColumns();
            _firstElement = true;
            if (!_minMax)
                return;
            _min = new ushort[_componentCount];
            _max = new ushort[_componentCount];
            for (var i = 0; i < _componentCount; i++)
            {
                _min[i] = ushort.MaxValue;
                _max[i] = ushort.MinValue;
            }
        }

        public int Index { get; }

        public GltfAsset GltfAsset { get; }

        public BufferView BufferView { get; }

        public int? ByteOffset => _actualByteOffset != 0 ? _actualByteOffset : null;

        public ComponentType ComponentType { get; }

        public bool? Normalized { get; }

        public int Count { get; private set; }

        public AccessorType Type { get; }

        public IEnumerable Max => _max;

        public IEnumerable Min => _min;

        public string Name { get; }

        public void Write(params ushort[] components)
        {
            if (BufferView.Target == BufferViewTarget.ArrayBuffer)
                BufferView.BinaryWriter.Seek((int) BufferView.BinaryWriter.BaseStream.Position.GetOffset(4),
                    SeekOrigin.Current);
            if (_firstElement)
                _actualByteOffset = (int) BufferView.BinaryWriter.BaseStream.Position;
            var element = new List<ushort>(_componentCount);
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
        }
    }
}