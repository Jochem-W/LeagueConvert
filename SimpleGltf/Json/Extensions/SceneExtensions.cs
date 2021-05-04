using System.Collections.Generic;

namespace SimpleGltf.Json.Extensions
{
    public static class SceneExtensions
    {
        public static void AddNode(this Scene scene, Node node)
        {
            scene.Nodes ??= new List<Node>();
            scene.Nodes.Add(node);
        }
    }
}