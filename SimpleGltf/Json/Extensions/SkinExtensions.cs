namespace SimpleGltf.Json.Extensions
{
    public static class SkinExtensions
    {
        public static void AddJoint(this Skin skin, Node node)
        {
            skin.JointList.Add(node);
        }
    }
}