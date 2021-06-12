using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Image : IIndexable
    {
        private Image(GltfAsset gltfAsset)
        {
            gltfAsset.Images ??= new List<Image>();
            Index = gltfAsset.Images.Count;
            gltfAsset.Images.Add(this);
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

        [JsonIgnore] public int Index { get; }
        
        public string Uri { get; }

        [JsonConverter(typeof(MimeTypeConverter))]
        public MimeType? MimeType { get; }

        [JsonConverter(typeof(IndexableConverter<BufferView>))] public BufferView BufferView { get; }

        public string Name { get; }
    }
}