using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class BufferView : IIndexable
    {
        internal readonly BinaryWriter BinaryWriter;
        internal readonly GltfAsset GltfAsset;
        internal int ActualByteOffset;
        internal int ActualByteStride;
        internal bool Stride;

        internal BufferView(Buffer buffer)
        {
            Buffer = buffer;
            GltfAsset = buffer.GltfAsset;
            GltfAsset.BufferViews ??= new List<BufferView>();
            Index = GltfAsset.BufferViews.Count;
            GltfAsset.BufferViews.Add(this);
            BinaryWriter = new BinaryWriter(new MemoryStream());
            ActualByteStride = 0;
            Stride = true;
        }
        
        [JsonIgnore] public int Index { get; }

        [JsonConverter(typeof(IndexableConverter<Buffer>))] public Buffer Buffer { get; }

        public int? ByteOffset => ActualByteOffset != 0 ? ActualByteOffset : null;

        public int ByteLength => (int) BinaryWriter.BaseStream.Length;

        public int? ByteStride => ActualByteStride != 0 ? ActualByteStride : null;

        public BufferViewTarget? Target { get; init; }

        public void StopStride()
        {
            Stride = false;
        }
    }
}