using System.Numerics;

namespace SimpleGltf.Json
{
    public class PbrMetallicRoughness
    {
        private Vector4 _baseColorFactor;
        private float _metallicFactor;
        private float _roughnessFactor;

        public PbrMetallicRoughness(Vector4? baseColorFactor, TextureInfo baseColorTexture, float? metallicFactor,
            float? roughnessFactor, TextureInfo metallicRoughnessTexture)
        {
            BaseColorFactor = baseColorFactor;
            BaseColorTexture = baseColorTexture;
            MetallicFactor = metallicFactor;
            RoughnessFactor = roughnessFactor;
            MetallicRoughnessTexture = metallicRoughnessTexture;
        }

        public Vector4? BaseColorFactor
        {
            get => _baseColorFactor == Vector4.One ? null : _baseColorFactor;
            internal set => _baseColorFactor = value ?? Vector4.One;
        }

        public TextureInfo BaseColorTexture { get; set; }

        public float? MetallicFactor
        {
            get => _metallicFactor is 1f ? null : _metallicFactor;
            internal set => _metallicFactor = value ?? 1f;
        }

        public float? RoughnessFactor
        {
            get => _roughnessFactor is 1f ? null : _roughnessFactor;
            internal set => _roughnessFactor = value ?? 1f;
        }

        public TextureInfo MetallicRoughnessTexture { get; set; }
    }
}