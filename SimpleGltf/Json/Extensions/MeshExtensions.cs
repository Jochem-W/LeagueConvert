namespace SimpleGltf.Json.Extensions
{
    internal static class MeshExtensions
    {
        internal static Primitive CreatePrimitive(this Mesh mesh)
        {
            return new(mesh);
        }
    }
}