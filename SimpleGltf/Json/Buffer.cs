using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    public class Buffer : IIndexable
    {
        internal readonly GltfAsset GltfAsset;
        internal Stream Stream;
        internal IList<BufferView> BufferViews;

        internal Buffer(GltfAsset gltfAsset)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Buffers ??= new List<Buffer>();
            Index = GltfAsset.Buffers.Count;
            GltfAsset.Buffers.Add(this);
            Stream = null;
            BufferViews = new List<BufferView>();
        }

        public string Uri { get; internal set; }

        public int ByteLength => (int) Stream.Length;

        [JsonIgnore] public int Index { get; }
    }
}