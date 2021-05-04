using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json
{
    public class GltfAsset
    {
        public GltfAsset()
        {
            this.CreateAsset();
        }

        internal Scene Scene { get; set; }

        public IList<Accessor> Accessors { get; internal set; }

        public Asset Asset { get; internal set; }

        public IList<Buffer> Buffers { get; internal set; }

        public IList<BufferView> BufferViews { get; internal set; }

        public IList<Image> Images { get; internal set; }

        public IList<Material> Materials { get; internal set; }

        public IList<Mesh> Meshes { get; internal set; }

        public IList<Node> Nodes { get; internal set; }

        public IList<Sampler> Samplers { get; internal set; }

        [JsonPropertyName("scene")] public int? SceneReference => Scene != null ? Scenes.IndexOf(Scene) : null;

        public IList<Scene> Scenes { get; internal set; }

        public IList<Texture> Textures { get; internal set; }
    }
}