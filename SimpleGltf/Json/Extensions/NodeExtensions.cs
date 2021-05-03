namespace SimpleGltf.Json.Extensions
{
    internal static class NodeExtensions
    {
        internal static Node CreateChild(this Node node, string name = null)
        {
            return new(node, name);
        }

        internal static Mesh CreateMesh(this Node node, string name = null)
        {
            return new(node, name);
        }
    }
}