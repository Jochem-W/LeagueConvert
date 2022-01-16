namespace SimpleGltf.Json.Extensions;

public static class MeshExtensions
{
    public static MeshPrimitive CreatePrimitive(this Mesh mesh)
    {
        return new MeshPrimitive(mesh);
    }
}