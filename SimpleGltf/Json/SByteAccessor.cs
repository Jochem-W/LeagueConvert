using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json
{
    public class SByteAccessor : IAccessor
    {
        private readonly int _columns;
        private readonly int _componentCount;
        private readonly bool _minMax;
        private readonly int _rows;
        private readonly List<sbyte> _values;

        internal SByteAccessor(BufferView bufferView, AccessorType type, bool minMax, bool normalized, string name)
        {
            BufferView = bufferView;
            GltfAsset = BufferView.GltfAsset;
            GltfAsset.Accessors ??= new List<IAccessor>();
            GltfAsset.Accessors.Add(this);
            ComponentType = ComponentType.SByte;
            Type = type;
            if (normalized)
                Normalized = true;
            Name = name;
            _componentCount = Type.GetColumns() * Type.GetRows();
            ComponentTypeLength = ComponentType.GetSize();
            ElementSize = _componentCount * ComponentTypeLength;
            _minMax = minMax;
            _rows = Type.GetRows();
            _columns = Type.GetColumns();
            _values = new List<sbyte>();
        }

        public GltfAsset GltfAsset { get; }

        public BufferView BufferView { get; }

        public int? ByteOffset { get; set; }

        public ComponentType ComponentType { get; }

        public bool? Normalized { get; }

        public int Count { get; private set; }

        public AccessorType Type { get; }

        public IEnumerable Max
        {
            get
            {
                if (!_minMax)
                    return null;
                var elements = _values.Batch(_componentCount).ToList();
                var value = elements[0];
                foreach (var element in elements.Skip(1))
                    for (var i = 0; i < _componentCount; i++)
                        if (element[i].CompareTo(value[i]) > 0)
                            value[i] = element[i];
                return value;
            }
        }

        public IEnumerable Min
        {
            get
            {
                if (!_minMax)
                    return null;
                var elements = _values.Batch(_componentCount).ToList();
                var value = elements[0];
                foreach (var element in elements.Skip(1))
                    for (var i = 0; i < _componentCount; i++)
                        if (element[i].CompareTo(value[i]) < 0)
                            value[i] = element[i];
                return value;
            }
        }

        public string Name { get; }

        public int ComponentTypeLength { get; }

        public int ElementSize { get; }

        public void WriteToBinaryWriter(BinaryWriter binaryWriter, int index)
        {
            var offset = index * _componentCount;
            for (var i = 0; i < _componentCount; i++)
                binaryWriter.Write(_values[i + offset]);
        }

        public void Write(params sbyte[] components)
        {
            if (Type != AccessorType.Mat2 &&
                Type != AccessorType.Mat3 &&
                Type != AccessorType.Mat4)
            {
                _values.AddRange(components);
                Count++;
                return;
            }

            for (var i = 0; i < _columns; i++)
            for (var j = 0; j < _rows; j++)
                _values.Add(components[_rows * j + i]);
            Count++;
        }
    }
}