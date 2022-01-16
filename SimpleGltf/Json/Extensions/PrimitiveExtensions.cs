namespace SimpleGltf.Json.Extensions;

public static class PrimitiveExtensions
{
    public static MeshPrimitive SetAttribute(this MeshPrimitive meshPrimitive, string attribute, Accessor accessor)
    {
        if (accessor != null) meshPrimitive.AttributesDictionary[attribute] = accessor;
        return meshPrimitive;
    }
}