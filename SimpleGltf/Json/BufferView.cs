using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json
{
    internal class BufferView
    {
        internal readonly BinaryWriter BinaryWriter;
        internal readonly GltfAsset GltfAsset;
        private int? _byteOffset;

        internal BufferView(GltfAsset gltfAsset, BufferViewTarget target)
        {
            BinaryWriter = new BinaryWriter(new MemoryStream());
            GltfAsset = gltfAsset;
            Target = (int) target;
            GltfAsset.BufferViews ??= new List<BufferView>();
            GltfAsset.BufferViews.Add(this);
        }

        internal IEnumerable<Accessor> Accessors => GltfAsset.Accessors.Where(accessor => accessor.BufferView == this);

        public int? ByteOffset
        {
            get => _byteOffset == 0 ? null : _byteOffset;
            set => _byteOffset = value;
        }

        public int? ByteLength => (int) BinaryWriter.BaseStream.Length;

        public int? ByteStride
        {
            get
            {
                if (Target == (int) BufferViewTarget.ElementArrayBuffer || Accessors.Count() == 1)
                    return null;
                return Accessors.GetStride();
            }
        }

        public int? Target { get; }

        [JsonPropertyName("buffer")]
        public int? BufferReference
        {
            get
            {
                if (GltfAsset.Buffers == null)
                    return null;
                return 0;
            }
        }
    }
}