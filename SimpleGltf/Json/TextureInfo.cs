using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    public class TextureInfo
    {
        private const int TexCoordDefault = 0;
        private int _texCoord = TexCoordDefault;

        internal TextureInfo(Texture texture)
        {
            Texture = texture;
        }

        public int? TexCoord
        {
            get => _texCoord == TexCoordDefault ? null : _texCoord;
            set => _texCoord = value ?? TexCoordDefault;
        }

        [JsonIgnore] public Texture Texture { get; }

        public int Index => Texture.Index;
    }
}