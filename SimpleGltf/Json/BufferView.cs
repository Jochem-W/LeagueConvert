using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Enums;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json
{
    internal class BufferView
    {
        internal readonly Buffer Buffer;

        internal BufferView(Buffer buffer, BufferViewTarget target, string name)
        {
            Buffer = buffer;
            Target = target;
            Name = name;
            Buffer.GltfAsset.BufferViews ??= new List<BufferView>();
            Buffer.GltfAsset.BufferViews.Add(this);
        }

        [JsonPropertyName("buffer")] public int BufferReference => Buffer.GltfAsset.Buffers.IndexOf(Buffer);

        public int? ByteOffset => this.GetByteOffset();

        public int ByteLength => (int) this.GetAccessors().GetLength();

        public int? ByteStride
        {
            get
            {
                var accessors = this.GetAccessors().ToList();
                return Target == BufferViewTarget.ElementArrayBuffer || accessors.Count == 1
                    ? null
                    : accessors.GetStride();
            }
        }

        public BufferViewTarget Target { get; }

        public string Name { get; }
    }
}