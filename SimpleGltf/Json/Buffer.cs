using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    public class Buffer : IIndexable
    {
        internal readonly GltfAsset GltfAsset;
        internal Stream Stream;

        internal Buffer(GltfAsset gltfAsset)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Buffers ??= new List<Buffer>();
            Index = GltfAsset.Buffers.Count;
            GltfAsset.Buffers.Add(this);
            Stream = null;
        }
        
        [JsonIgnore] public int Index { get; }

        public string Uri { get; internal set; }

        public int ByteLength => (int) Stream.Length;
    }
}