using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json
{
    public class Accessor : IAsyncDisposable
    {
        private readonly GltfAsset _gltfAsset;
        private readonly bool _normalized;

        internal readonly BinaryWriter BinaryWriter;
        internal readonly IList<dynamic> Component;
        internal readonly bool MinMax;
        internal BufferView BufferView;

        internal Accessor(GltfAsset gltfAsset, ComponentType componentType, AccessorType accessorType,
            bool normalized, string name, bool minMax)
        {
            _gltfAsset = gltfAsset;
            _gltfAsset.Accessors ??= new List<Accessor>();
            _gltfAsset.Accessors.Add(this);
            ComponentType = componentType;
            Type = accessorType;
            _normalized = normalized;
            Name = name;
            MinMax = minMax;
            Component = new List<dynamic>();
            BinaryWriter = new BinaryWriter(new MemoryStream());
            this.SetSize();
        }

        internal int ComponentSize { get; set; }
        internal int ByteLength => ComponentSize * Count;

        [JsonPropertyName("bufferView")]
        public int? BufferViewReference => BufferView == null ? null : _gltfAsset.BufferViews.IndexOf(BufferView);

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
                if (accessors.Count == 0)
                    return null;
                return accessors.GetStride();
            }
        }

        public ComponentType ComponentType { get; }

        public bool? Normalized => _normalized == false ? null : _normalized;

        public int Count { get; internal set; }

        [JsonConverter(typeof(AccessorTypeConverter))]
        public AccessorType Type { get; }

        public dynamic Max { get; internal set; }

        public dynamic Min { get; internal set; }

        public string Name { get; }

        public async ValueTask DisposeAsync()
        {
            await BinaryWriter.DisposeAsync();
        }

        internal void Write(dynamic value)
        {
            BinaryWriter.Write(value);
            Component.Add(value);
        }
    }
}