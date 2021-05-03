using System.Collections.Generic;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json
{
    internal class Buffer
    {
        internal readonly GltfAsset GltfAsset;

        internal Buffer(GltfAsset gltfAsset, string name)
        {
            GltfAsset = gltfAsset;
            Name = name;
            GltfAsset.Buffers ??= new List<Buffer>();
            GltfAsset.Buffers.Add(this);
        }

        public string Uri { get; internal set; }

        public int ByteLength => this.GetBufferViews().GetLength();

        public string Name { get; }
    }
}