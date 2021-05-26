using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SimpleGltf.Converters;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json
{
    public class Accessor : IAsyncDisposable
    {
        private readonly GltfAsset _gltfAsset;
        internal IList<dynamic> Element;
        
        internal readonly BinaryWriter BinaryWriter;
        internal readonly int ElementSize;

        internal Accessor(GltfAsset gltfAsset, ComponentType componentType, AccessorType type)
        {
            _gltfAsset = gltfAsset;
            _gltfAsset.Accessors ??= new List<Accessor>();
            _gltfAsset.Accessors.Add(this);
            ComponentType = componentType;
            Type = type;
            BinaryWriter = new BinaryWriter(new MemoryStream());
            this.GetElementCountAndSize(out _, out ElementSize);
        }
        
        [JsonConverter(typeof(ComponentTypeConverter))] public ComponentType ComponentType { get; }

        public int Count { get; internal set; }
        
        [JsonConverter(typeof(AccessorTypeConverter))] public AccessorType Type { get; }
        
        public ValueTask DisposeAsync()
        {
            return BinaryWriter.DisposeAsync();
        }
    }
}