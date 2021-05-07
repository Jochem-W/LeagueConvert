using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json
{
    public class BufferView
    {
        private readonly GltfAsset _gltfAsset;

        internal readonly Buffer Buffer;
        internal Stream PngStream;
        internal IList<IList<Accessor>> AccessorGroups;

        internal BufferView(GltfAsset gltfAsset, Buffer buffer)
        {
            _gltfAsset = gltfAsset;
            _gltfAsset.BufferViews ??= new List<BufferView>();
            _gltfAsset.BufferViews.Add(this);
            Buffer = buffer;
        }

        [JsonPropertyName("buffer")] public int BufferReference => _gltfAsset.Buffers.IndexOf(Buffer);

        public int? ByteOffset => this.GetByteOffset();

        public int ByteLength => PngStream != null
            ? (int) PngStream.Length
            : (int) AccessorGroups.SelectMany(group => group).GetLength();

        public int? ByteStride
        {
            get
            {
                if (Target is BufferViewTarget.ElementArrayBuffer or null)
                    return null;
                return AccessorGroups[0].Count == 1 ? null : AccessorGroups[0].GetStride();
            }
        }

        public BufferViewTarget? Target { get; init; }

        public string Name { get; set; }
    }
}