using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Accessor : IIndexable
    {
        internal int ActualByteOffset;

        internal Accessor(BufferView bufferView, AccessorType type, bool normalized)
        {
            BufferView = bufferView;
            GltfAsset = BufferView.GltfAsset;
            GltfAsset.Accessors ??= new List<Accessor>();
            Index = GltfAsset.Accessors.Count;
            GltfAsset.Accessors.Add(this);
            Type = type;
            if (normalized)
                Normalized = true;
        }

        [JsonIgnore] public int Index { get; }

        [JsonIgnore] public GltfAsset GltfAsset { get; }

        [JsonConverter(typeof(IndexableConverter<BufferView>))] public BufferView BufferView { get; }

        public int? ByteOffset => ActualByteOffset != 0 ? ActualByteOffset : null;

        public ComponentType ComponentType { get; init; }

        public bool? Normalized { get; }

        public int Count { get; internal set; }

        [JsonConverter(typeof(AccessorTypeConverter))]
        public AccessorType Type { get; }

        public IEnumerable Max { get; internal set; }

        public IEnumerable Min { get; internal set; }

        public string Name { get; }
    }
}