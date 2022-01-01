using System.Reflection;

namespace SimpleGltf.Json;

public class Asset
{
    internal Asset(GltfAsset gltfAsset)
    {
        gltfAsset.Asset = this;
    }

    public string Copyright { get; init; }

    public string Generator
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly().GetName();
            return $"{assembly.Name}@{assembly.Version}";
        }
    }

    public string Version => "2.0";
}