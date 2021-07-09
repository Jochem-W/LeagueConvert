namespace SimpleGltf.Json.Extensions
{
    public static class SceneExtensions
    {
        public static void AddNode(this Scene scene, Node node)
        {
            scene.NodeList.Add(node);
        }
    }
}