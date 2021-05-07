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
        private readonly bool _normalized;
        internal readonly BinaryWriter BinaryWriter;
        internal readonly IList<dynamic> Component;

        internal readonly GltfAsset GltfAsset;
        internal readonly bool MinMax;
        internal BufferView BufferView;

        internal Accessor(GltfAsset gltfAsset, ComponentType componentType, AccessorType accessorType,
            bool normalized, string name, bool minMax)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Accessors ??= new List<Accessor>();
            GltfAsset.Accessors.Add(this);
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
        public int? BufferViewReference => BufferView == null ? null : GltfAsset.BufferViews.IndexOf(BufferView);

        public int? ByteOffset
        {
            get
            {
                var take = true;
                var groups = BufferView.AccessorGroups.TakeWhile(group =>
                {
                    if (group.Contains(this))
                        take = false;
                    return take;
                }).ToList();
                var currentGroup = BufferView.AccessorGroups[groups.Count];
                if (currentGroup.Count == 1)
                    return null;
                var offset = groups.SelectMany(group => group).GetLength();
                take = true;
                var inGroupBefore = currentGroup.TakeWhile(accessor =>
                {
                    if (accessor == this)
                        take = false;
                    return take;
                }).ToList();
                return inGroupBefore.Count switch
                {
                    0 when offset == 0 => null,
                    0 => (int) offset,
                    _ => inGroupBefore.GetStride() + (int) offset
                };
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