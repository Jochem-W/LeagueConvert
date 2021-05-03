using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json
{
    internal class Buffer
    {
        internal readonly GltfAsset GltfAsset;

        internal Buffer(GltfAsset gltfAsset)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Buffers ??= new List<Buffer>();
            GltfAsset.Buffers.Add(this);
        }

        public string Uri { get; set; }

        public int ByteLength => this.GetBufferViews().GetLength();

        public string Name { get; set; }

        internal Stream GetStream()
        {
            using var task = GetStreamAsync();
            task.Wait();
            return task.Result;
        }

        internal async Task<Stream> GetStreamAsync()
        {
            var stream = new MemoryStream();
            foreach (var bufferView in GltfAsset.BufferViews.Where(bufferView => bufferView.Buffer == this))
            {
                stream.Seek(bufferView.ByteOffset ?? 0, SeekOrigin.Begin);
                await using var bufferViewStream = await bufferView.GetStreamAsync();
                await bufferViewStream.CopyToAsync(stream);
            }

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}