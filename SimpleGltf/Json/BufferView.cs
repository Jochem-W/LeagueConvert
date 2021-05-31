using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;

namespace SimpleGltf.Json
{
    public class BufferView
    {
        internal readonly Buffer Buffer;
        internal readonly GltfAsset GltfAsset;
        internal MemoryStream PngStream;

        internal BufferView(Buffer buffer, BufferViewTarget? target, string name)
        {
            Buffer = buffer;
            GltfAsset = buffer.GltfAsset;
            GltfAsset.BufferViews ??= new List<BufferView>();
            GltfAsset.BufferViews.Add(this);
            Target = target;
            Name = name;
        }

        [JsonPropertyName("buffer")] public int BufferReference => GltfAsset.Buffers.IndexOf(Buffer);

        public int ByteOffset { get; internal set; }

        public int ByteLength { get; internal set; }

        public int? ByteStride { get; internal set; }

        public BufferViewTarget? Target { get; }

        public string Name { get; }
    }
}