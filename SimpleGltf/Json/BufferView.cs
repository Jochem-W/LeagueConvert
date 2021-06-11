using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;

namespace SimpleGltf.Json
{
    public class BufferView
    {
        internal readonly BinaryWriter BinaryWriter;
        internal readonly GltfAsset GltfAsset;
        internal readonly int Index;
        internal int ActualByteOffset;
        internal int ActualByteStride;
        internal MemoryStream PngStream;
        internal bool Stride;

        internal BufferView(Buffer buffer, BufferViewTarget? target, string name)
        {
            Buffer = buffer;
            GltfAsset = buffer.GltfAsset;
            GltfAsset.BufferViews ??= new List<BufferView>();
            Index = GltfAsset.BufferViews.Count;
            GltfAsset.BufferViews.Add(this);
            Target = target;
            Name = name;
            BinaryWriter = new BinaryWriter(new MemoryStream());
            ActualByteStride = 0;
            Stride = true;
        }

        [JsonIgnore] public Buffer Buffer { get; }

        [JsonPropertyName("buffer")] public int BufferIndex => Buffer.Index;

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