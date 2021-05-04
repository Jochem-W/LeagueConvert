using System.Reflection;

namespace SimpleGltf.Json
{
    public class Asset
    {
        private readonly GltfAsset _gltfAsset;

        internal Asset(GltfAsset gltfAsset, string copyright)
        {
            _gltfAsset = gltfAsset;
            _gltfAsset.Asset = this;
            Copyright = copyright;
            var assembly = Assembly.GetExecutingAssembly().GetName();
            Generator = $"{assembly.Name}@{assembly.Version}";
            Version = "2.0";
        }

        public string Copyright { get; internal set; }

        public string Generator { get; }

        public string Version { get; }
    }
}