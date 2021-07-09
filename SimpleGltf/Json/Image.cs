using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Image : IIndexable
    {
        private Image(GltfAsset gltfAsset)
        {
            Index = gltfAsset.ImageList.Count;
            gltfAsset.ImageList.Add(this);
        }

        internal Image(GltfAsset gltfAsset, BufferView bufferView, MimeType mimeType, string name) : this(gltfAsset)
        {
            BufferView = bufferView;
            MimeType = mimeType;
            Name = name;
        }

        public string Uri { get; internal set; }

        [JsonConverter(typeof(MimeTypeConverter))]
        public MimeType? MimeType { get; }

        [JsonConverter(typeof(IndexableConverter<BufferView>))]
        public BufferView BufferView { get; }

        public string Name { get; }

        [JsonIgnore] public int Index { get; }
    }
}