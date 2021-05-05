namespace SimpleGltf.Json
{
    public class TextureInfo
    {
        private const int TexCoordDefault = 0;
        private readonly Texture _texture;
        private int _texCoord = TexCoordDefault;

        internal TextureInfo(Texture texture)
        {
            _texture = texture;
        }

        public int? TexCoord
        {
            get => _texCoord == TexCoordDefault ? null : _texCoord;
            set => _texCoord = value ?? TexCoordDefault;
        }

        public int Index => _texture.GltfAsset.Textures.IndexOf(_texture);
    }
}