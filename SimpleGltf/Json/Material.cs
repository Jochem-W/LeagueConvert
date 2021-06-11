using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Material
    {
        internal readonly int Index;
        internal readonly GltfAsset GltfAsset;
        private float _alphaCutoff;
        private AlphaMode _alphaMode;
        private bool _doubleSided;
        private Vector3? _emissiveVector;

        internal Material(GltfAsset gltfAsset, Vector3? emissiveFactor, AlphaMode? alphaMode,
            float? alphaCutoff, bool? doubleSided, string name)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Materials ??= new List<Material>();
            Index = GltfAsset.Materials.Count;
            GltfAsset.Materials.Add(this);
            EmissiveFactor = emissiveFactor;
            AlphaMode = alphaMode;
            AlphaCutoff = alphaCutoff;
            DoubleSided = doubleSided;
            Name = name;
        }

        public string Name { get; }

        public PbrMetallicRoughness PbrMetallicRoughness { get; internal set; }

        public TextureInfo EmissiveTexture { get; }

        public Vector3? EmissiveFactor
        {
            get => _emissiveVector == Vector3.Zero ? null : _emissiveVector;
            set => _emissiveVector = value;
        }

        [JsonConverter(typeof(AlphaModeConverter))]
        public AlphaMode? AlphaMode
        {
            get => _alphaMode == Enums.AlphaMode.Opaque ? null : _alphaMode;
            set => _alphaMode = value ?? Enums.AlphaMode.Opaque;
        }

        public float? AlphaCutoff
        {
            get => _alphaCutoff is 0.5f ? null : _alphaCutoff;
            set => _alphaCutoff = value ?? 0.5f;
        }

        public bool? DoubleSided
        {
            get => _doubleSided == false ? null : _doubleSided;
            set => _doubleSided = value ?? false;
        }
    }
}