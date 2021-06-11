using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json
{
    public class BufferView
    {
        internal readonly GltfAsset GltfAsset;
        internal MemoryStream PngStream;
        internal BinaryWriter BinaryWriter;
        internal int ActualByteStride;
        internal int ActualByteOffset;
        internal bool Stride;

        internal BufferView(Buffer buffer, BufferViewTarget? target, string name)
        {
            Buffer = buffer;
            GltfAsset = buffer.GltfAsset;
            GltfAsset.BufferViews ??= new List<BufferView>();
            GltfAsset.BufferViews.Add(this);
            Target = target;
            Name = name;
            BinaryWriter = new BinaryWriter(new MemoryStream());
            ActualByteStride = 0;
            Stride = true;
        }

        [JsonConverter(typeof(BufferConverter))]
        public Buffer Buffer { get; }

        public int? ByteOffset => ActualByteOffset != 0 ? ActualByteOffset : null;

        public int ByteLength { get; internal set; }

        public int? ByteStride => ActualByteStride != 0 ? ActualByteStride : null;

        public BufferViewTarget? Target { get; }

        public string Name { get; }

        public void StopStride()
        {
            Stride = false;
        }
    }
}