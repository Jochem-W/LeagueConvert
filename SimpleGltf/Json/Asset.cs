using System.Reflection;

namespace SimpleGltf.Json
{
    internal class Asset
    {
        internal Asset(GltfAsset gltfAsset, string copyright)
        {
            Copyright = copyright;
            var assembly = Assembly.GetExecutingAssembly().GetName();
            Generator = $"{assembly.Name}@{assembly.Version}";
            Version = "2.0";
            gltfAsset.Asset = this;
        }

        public string Copyright { get; internal set; }

        public string Generator { get; }

        public string Version { get; }
    }
}