using System.Collections.Generic;

namespace SimpleGltf.Json.Extensions
{
    public static class NodeExtensions
    {
        public static void AddChild(this Node node, Node child)
        {
            node.Children ??= new List<Node>();
            node.Children.Add(child);
        }
    }
}