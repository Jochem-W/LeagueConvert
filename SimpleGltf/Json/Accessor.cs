using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SimpleGltf.Json.Converters;
using SimpleGltf.Json.Enums;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json
{
    internal class Accessor : IAsyncDisposable
    {
        internal readonly BufferView BufferView;
        internal readonly IList<dynamic> Component;
        internal readonly bool MinMax;
        internal BinaryWriter BinaryWriter;

        internal Accessor(BufferView bufferView, ComponentType componentType, AccessorType accessorType,
            bool? normalized, string name, bool minMax)
        {
            Component = new List<dynamic>();
            MinMax = minMax;
            BinaryWriter = new BinaryWriter(new MemoryStream());
            BufferView = bufferView;
            ComponentType = componentType;
            Type = accessorType;
            Normalized = normalized;
            this.SetSize();
            BufferView.Buffer.GltfAsset.Accessors ??= new List<Accessor>();
            BufferView.Buffer.GltfAsset.Accessors.Add(this);
        }

        internal int ComponentSize { get; set; }
        internal int ByteLength => ComponentSize * Count;

        [JsonPropertyName("bufferView")]
        public int? BufferViewReference => BufferView.Buffer.GltfAsset.BufferViews.IndexOf(BufferView);

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

        public bool? Normalized { get; }

        public int Count { get; internal set; }

        [JsonConverter(typeof(AccessorTypeConverter))]
        public AccessorType Type { get; }

        public dynamic Max { get; internal set; }

        public dynamic Min { get; internal set; }

        //public IDictionary<?, ?> Sparse { get; private set; }

        public string Name { get; }

        public async ValueTask DisposeAsync()
        {
            if (BinaryWriter == null)
                return;
            await BinaryWriter.DisposeAsync();
            BinaryWriter = null;
        }

        internal void Write(dynamic value)
        {
            BinaryWriter.Write(value);
            Component.Add(value);
        }
    }
}