namespace SimpleGltf.Json.Extensions
{
    public static class NodeExtensions
    {
        public static void AddChild(this Node node, Node child)
        {
            node.ChildrenList.Add(child);
        }
    }
}