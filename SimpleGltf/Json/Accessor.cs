using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SimpleGltf.Converters;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json
{
    internal class Accessor : IAsyncDisposable
    {
        private readonly IList<dynamic> _component;
        private readonly bool _minMax;
        internal readonly BufferView BufferView;

        internal BinaryWriter BinaryWriter;

        internal Accessor(BufferView bufferView, ComponentType componentType, AccessorType accessorType,
            bool minMax, bool? normalized)
        {
            _component = new List<dynamic>();
            _minMax = minMax;
            BinaryWriter = new BinaryWriter(new MemoryStream());
            BufferView = bufferView;
            ComponentType = componentType;
            Type = accessorType;
            Normalized = normalized;
            SetSize();
            BufferView.Buffer.GltfAsset.Accessors ??= new List<Accessor>();
            BufferView.Buffer.GltfAsset.Accessors.Add(this);
        }

        internal int ComponentSize { get; private set; }
        internal int ByteLength => ComponentSize * Count;

        [JsonPropertyName("bufferView")]
        public int BufferViewReference => BufferView.Buffer.GltfAsset.BufferViews.IndexOf(BufferView);

        public int? ByteOffset
        {
            get
            {
                var take = true;
                var accessors = BufferView.GetAccessors().TakeWhile(accessor =>
                {
                    if (accessor == this)
                        take = false;
                    return take;
                }).ToList();
                if (!accessors.Any())
                    return null;
                return accessors.GetStride();
            }
        }

        public ComponentType ComponentType { get; }

        public bool? Normalized { get; }

        public int Count { get; private set; }

        [JsonConverter(typeof(AccessorTypeConverter))]
        public AccessorType Type { get; }

        public dynamic Min { get; private set; }

        public dynamic Max { get; private set; }

        public async ValueTask DisposeAsync()
        {
            if (BinaryWriter == null)
                return;
            await BinaryWriter.DisposeAsync();
            BinaryWriter = null;
        }

        private void SetSize()
        {
            var elementSize = ComponentType.GetElementSize();
            var rows = Type.GetRows();
            var columns = Type.GetColumns();
            for (var i = 0; i < columns; i++)
            {
                ComponentSize += ComponentSize.GetOffset();
                ComponentSize += rows * elementSize;
            }
        }

        internal void NextComponent()
        {
            Count++;
            if (!_minMax)
            {
                _component.Clear();
                return;
            }

            if (_component.Count == 1)
            {
                if (Min == null || Max == null)
                {
                    Min = _component[0];
                    Max = _component[0];
                    _component.Clear();
                    return;
                }

                if (_component[0] < Min)
                    Min = _component[0];
                if (_component[0] > Max)
                    Max = _component[0];
                _component.Clear();
                return;
            }

            if (Min == null && Max == null)
            {
                Min = new List<dynamic>(_component);
                Max = new List<dynamic>(_component);
                _component.Clear();
                return;
            }

            for (var i = 0; i < _component.Count; i++)
            {
                if (_component[i] < Min[i])
                    Min[i] = _component[i];
                if (_component[i] > Max[i])
                    Max[i] = _component[i];
            }

            _component.Clear();
        }

        internal void Write(dynamic value)
        {
            BinaryWriter.Write(value);
            _component.Add(value);
        }
    }
}