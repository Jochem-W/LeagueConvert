using System.Collections.Generic;
using System.IO;

namespace SimpleGltf.Json
{
    public class Buffer
    {
        internal readonly GltfAsset GltfAsset;
        internal MemoryStream MemoryStream;

        internal Buffer(GltfAsset gltfAsset, string name)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Buffers ??= new List<Buffer>();
            GltfAsset.Buffers.Add(this);
            Name = name;
            MemoryStream = null;
        }

        public string Uri { get; internal set; }

        public int ByteLength => (int) MemoryStream.Length;

        public string Name { get; }
    }
}