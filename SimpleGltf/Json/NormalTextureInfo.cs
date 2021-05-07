namespace SimpleGltf.Json
{
    public class NormalTextureInfo : TextureInfo
    {
        private const int ScaleDefault = 1;
        private int _scale = ScaleDefault;

        internal NormalTextureInfo(Texture texture) : base(texture)
        {
        }

        public int? Scale
        {
            get => _scale == ScaleDefault ? null : _scale;
            set => _scale = value ?? ScaleDefault;
        }
    }
}