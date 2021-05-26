using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json
{
    public class BufferView
    {
        private const int DefaultByteOffset = 0;
        private readonly GltfAsset _gltfAsset;

        internal readonly Buffer Buffer;
        internal IList<IList<Accessor>> AccessorGroups;

        internal BufferView(GltfAsset gltfAsset, Buffer buffer)
        {
            _gltfAsset = gltfAsset;
            _gltfAsset.BufferViews ??= new List<BufferView>();
            _gltfAsset.BufferViews.Add(this);
            Buffer = buffer;
            Buffer.BufferViews ??= new List<BufferView>();
            Buffer.BufferViews.Add(this);
        }

        [JsonPropertyName("buffer")] public int BufferReference => _gltfAsset.Buffers.IndexOf(Buffer);

        public int? ByteOffset
        {
            get
            {
                var beforeCount = Buffer.BufferViews.IndexOf(this);
                if (beforeCount == 0)
                    return null;
                var lastBefore = Buffer.BufferViews.Take(beforeCount).Last();
                return lastBefore.ByteOffset + lastBefore.ByteLength;
            }
        }

        public int ByteLength => AccessorGroups.Sum(group => (int) group.GetByteLength());
    }
}