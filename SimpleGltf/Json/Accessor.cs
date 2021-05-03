using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SimpleGltf.Converters;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json
{
    internal class Accessor
    {
        private readonly IList<dynamic> _component;
        private readonly bool _minMax;

        internal BufferView BufferView;
        internal int Size;

        internal Accessor(BufferView bufferView, ComponentType componentType, AccessorType accessorType,
            bool minMax, bool? normalized)
        {
            _component = new List<dynamic>();
            _minMax = minMax;
            BufferView = bufferView;
            ComponentType = componentType;
            Type = accessorType;
            Normalized = normalized;
            BufferView.GltfAsset.Accessors ??= new List<Accessor>();
            BufferView.GltfAsset.Accessors.Add(this);
        }

        [JsonPropertyName("bufferView")]
        public int BufferViewReference => BufferView.GltfAsset.BufferViews.IndexOf(BufferView);

        public int? ByteOffset
        {
            get
            {
                var take = true;
                var accessors = BufferView.Accessors.TakeWhile(accessor =>
                {
                    if (accessor == this)
                        take = false;
                    return take;
                });
                if (!accessors.Any())
                    return null;
                return accessors.GetStride();
            }
        }

        public ComponentType ComponentType { get; }

        public bool? Normalized { get; }

        public int Count { get; set; }

        [JsonConverter(typeof(AccessorTypeConverter))]
        public AccessorType Type { get; }

        public dynamic Min { get; set; }

        public dynamic Max { get; set; }

        internal void NextComponent()
        {
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

            if (Min == null || Max == null)
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
            BufferView.BinaryWriter.Write(value);
            _component.Add(value);
        }
    }
}