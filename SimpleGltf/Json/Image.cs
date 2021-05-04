using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Image
    {
        private readonly BufferView _bufferView;
        private readonly GltfAsset _gltfAsset;

        private Image(GltfAsset gltfAsset)
        {
            _gltfAsset = gltfAsset;
            _gltfAsset.Images ??= new List<Image>();
            _gltfAsset.Images.Add(this);
        }

        internal Image(GltfAsset gltfAsset, BufferView bufferView, MimeType mimeType, string name) : this(gltfAsset)
        {
            _bufferView = bufferView;
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

        [JsonPropertyName("bufferView")]
        public int? BufferViewReference => _bufferView == null ? null : _gltfAsset.BufferViews.IndexOf(_bufferView);

        public string Name { get; }
    }
}