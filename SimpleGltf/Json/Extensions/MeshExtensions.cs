namespace SimpleGltf.Json.Extensions
{
    public static class MeshExtensions
    {
        public static Primitive CreatePrimitive(this Mesh mesh)
        {
            return new(mesh);
        }
    }
}