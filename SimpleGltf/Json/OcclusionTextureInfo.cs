namespace SimpleGltf.Json
{
    public class OcclusionTextureInfo : TextureInfo
    {
        internal OcclusionTextureInfo(Texture texture, int strength, int texCoord) : base(texture, texCoord)
        {
            Strength = strength;
        }

        public int Strength { get; }
    }
}