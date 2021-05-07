namespace SimpleGltf.Json
{
    public class OcclusionTextureInfo : TextureInfo
    {
        private const int StrengthDefault = 1;
        private int _strength = StrengthDefault;

        internal OcclusionTextureInfo(Texture texture) : base(texture)
        {
        }

        public int? Strength
        {
            get => _strength == StrengthDefault ? null : _strength;
            set => _strength = value ?? StrengthDefault;
        }
    }
}