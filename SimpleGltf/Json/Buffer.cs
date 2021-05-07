using System.Collections.Generic;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json
{
    public class Buffer
    {
        internal readonly GltfAsset GltfAsset;

        internal Buffer(GltfAsset gltfAsset)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Buffers ??= new List<Buffer>();
            GltfAsset.Buffers.Add(this);
        }

        public int ByteLength => this.GetBufferViews().GetLength();

        public string Name { get; set; }
    }
}