namespace SimpleGltf.Json.Extensions
{
    internal static class SceneExtensions
    {
        internal static Node CreateNode(this Scene scene, string name = null)
        {
            return new(scene, name);
        }
    }
}