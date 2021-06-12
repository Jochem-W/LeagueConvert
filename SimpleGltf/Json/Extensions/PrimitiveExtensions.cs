namespace SimpleGltf.Json.Extensions
{
    public static class PrimitiveExtensions
    {
        public static Primitive SetAttribute(this Primitive primitive, string attribute, Accessor accessor)
        {
            primitive.Attributes[attribute] = accessor;
            return primitive;
        }
    }
}