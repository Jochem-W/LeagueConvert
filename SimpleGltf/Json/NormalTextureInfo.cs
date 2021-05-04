namespace SimpleGltf.Json
{
    public class NormalTextureInfo : TextureInfo
    {
        internal NormalTextureInfo(Texture texture, int scale, int texCoord) : base(texture, texCoord)
        {
            Scale = scale;
        }

        public int Scale { get; }
    }
}