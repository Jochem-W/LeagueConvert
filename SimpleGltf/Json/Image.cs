using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Image
    {
        internal readonly int Index;
        internal readonly GltfAsset GltfAsset;

        private Image(GltfAsset gltfAsset)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Images ??= new List<Image>();
            Index = GltfAsset.Images.Count;
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

        [JsonIgnore] public BufferView BufferView { get; }

        [JsonPropertyName("bufferView")] public int BufferViewIndex => BufferView.Index;

        public string Name { get; }
    }
}