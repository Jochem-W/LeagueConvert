using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json
{
    internal class BufferView
    {
        internal readonly Buffer Buffer;

        internal BufferView(Buffer buffer, BufferViewTarget target)
        {
            Buffer = buffer;
            Target = (int) target;
            Buffer.GltfAsset.BufferViews ??= new List<BufferView>();
            Buffer.GltfAsset.BufferViews.Add(this);
        }

        public int? ByteOffset => this.GetByteOffset();

        public int ByteLength => (int) this.GetAccessors().GetLength();

        public int? ByteStride
        {
            get
            {
                var accessors = this.GetAccessors().ToList();
                if (Target == (int) BufferViewTarget.ElementArrayBuffer || accessors.Count == 1)
                    return null;
                return accessors.GetStride();
            }
        }

        public int? Target { get; }

        [JsonPropertyName("buffer")]
        public int? BufferReference
        {
            get
            {
                if (Buffer.GltfAsset.Buffers == null)
                    return null;
                return 0;
            }
        }

        internal async Task<Stream> GetStreamAsync()
        {
            var stream = new MemoryStream();
            var stride = ByteStride;
            var accessors = this.GetAccessors().ToList();
            accessors.SeekToBegin();
            if (stride == null)
            {
                foreach (var accessor in accessors)
                    await accessor.BinaryWriter.BaseStream.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }

            var totalLength = accessors.GetLength();
            while (stream.Length != totalLength)
                foreach (var accessor in accessors)
                {
                    var memory = new byte[accessor.ComponentSize];
                    await accessor.BinaryWriter.BaseStream.ReadAsync(memory);
                    await stream.WriteAsync(memory);
                }

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}