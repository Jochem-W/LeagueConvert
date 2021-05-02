using System.Reflection;

namespace SimpleGltf.Json
{
    internal class Asset
    {
        internal readonly GltfAsset GltfAsset;

        internal Asset(GltfAsset gltfAsset)
        {
            GltfAsset = gltfAsset;
            var assembly = Assembly.GetExecutingAssembly().GetName();
            Generator = $"{assembly.Name}@{assembly.Version}";
        }

        public string Version => "2.0";

        public string Generator { get; set; }

        public string Copyright { get; set; }
    }
}