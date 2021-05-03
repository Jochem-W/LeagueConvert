namespace SimpleGltf.Json.Extensions
{
    internal static class GltfAssetExtensions
    {
        internal static Buffer CreateBuffer(this GltfAsset gltfAsset, string name = null)
        {
            return new(gltfAsset, name);
        }

        internal static Asset CreateAsset(this GltfAsset gltfAsset, string copyright = null)
        {
            return new(gltfAsset, copyright);
        }

        internal static Scene CreateScene(this GltfAsset gltfAsset, string name = null, bool setDefault = true)
        {
            return new(gltfAsset, name, setDefault);
        }

        internal static Node CreateNode(this GltfAsset gltfAsset, string name = null)
        {
            return new(gltfAsset, name);
        }

        internal static Mesh CreateMesh(this GltfAsset gltfAsset, string name = null)
        {
            return new(gltfAsset, name);
        }
    }
}