using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleGltf.Json
{
    internal class Buffer : IAsyncDisposable
    {
        internal readonly GltfAsset GltfAsset;
        internal readonly MemoryStream MemoryStream;

        internal Buffer(GltfAsset gltfAsset)
        {
            MemoryStream = new MemoryStream();
            GltfAsset = gltfAsset;
            if (GltfAsset.Buffers != null && GltfAsset.Buffers.Any())
                throw new NotImplementedException();
            GltfAsset.Buffers = new List<Buffer>(1) {this};
        }

        public string Uri { get; set; }

        public int ByteLength => (int) MemoryStream.Length;

        public string Name { get; set; }

        public ValueTask DisposeAsync()
        {
            GltfAsset.Buffers.Remove(this);
            return MemoryStream.DisposeAsync();
        }
    }
}