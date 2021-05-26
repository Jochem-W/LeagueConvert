using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json
{
    public class Buffer
    {
        private readonly GltfAsset _gltfAsset;

        internal IList<BufferView> BufferViews;
        
        internal Buffer(GltfAsset gltfAsset)
        {
            _gltfAsset = gltfAsset;
            _gltfAsset.Buffers ??= new List<Buffer>();
            _gltfAsset.Buffers.Add(this);
        }

        public int ByteLength => _gltfAsset.BufferViews
            .Where(bufferView => bufferView.Buffer == this)
            .Sum(bufferView => bufferView.ByteLength);

        public async Task<Stream> GetStreamAsync()
        {
            var stream = new MemoryStream();
            foreach (var bufferView in BufferViews)
            foreach (var accessorGroup in bufferView.AccessorGroups)
            {
                var currentLength = stream.Length;
                var bytesToWrite = accessorGroup.GetByteLength();
                accessorGroup.Seek(0, SeekOrigin.Begin);
                while (stream.Length - currentLength != bytesToWrite) 
                    foreach (var accessor in accessorGroup) 
                        await accessor.BinaryWriter.BaseStream.CopyToAsync(stream, accessor.ElementSize);
            }

            return stream;
        }
    }
}