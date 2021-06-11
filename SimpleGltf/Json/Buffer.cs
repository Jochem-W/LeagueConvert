using System.Collections.Generic;
using System.IO;

namespace SimpleGltf.Json
{
    public class Buffer
    {
        internal readonly int Index;
        internal readonly GltfAsset GltfAsset;
        internal Stream Stream;

        internal Buffer(GltfAsset gltfAsset, string name)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Buffers ??= new List<Buffer>();
            Index = GltfAsset.Buffers.Count;
            GltfAsset.Buffers.Add(this);
            Name = name;
            Stream = null;
        }

        public string Uri { get; internal set; }

        public int ByteLength => (int) Stream.Length;

        public string Name { get; }
    }
}