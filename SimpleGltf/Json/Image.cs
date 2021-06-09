using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Image
    {
        internal readonly GltfAsset GltfAsset;

        private Image(GltfAsset gltfAsset)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Images ??= new List<Image>();
            GltfAsset.Images.Add(this);
        }

        internal Image(GltfAsset gltfAsset, BufferView bufferView, MimeType mimeType, string name) : this(gltfAsset)
        {
            BufferView = bufferView;
            MimeType = mimeType;
            Name = name;
        }

        internal Image(GltfAsset gltfAsset, string uri, string name) : this(gltfAsset)
        {
            Uri = uri;
            Name = name;
        }

        public string Uri { get; }

        [JsonConverter(typeof(MimeTypeConverter))]
        public MimeType? MimeType { get; }

        [JsonConverter(typeof(BufferViewConverter))]
        public BufferView BufferView { get; }

        public string Name { get; }
    }
}