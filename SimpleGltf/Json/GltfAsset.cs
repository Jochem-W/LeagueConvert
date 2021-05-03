using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Extensions;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json
{
    internal class GltfAsset
    {
        internal GltfAsset()
        {
            this.CreateAsset();
        }

        internal Scene Scene { get; set; }

        public IList<Accessor> Accessors { get; internal set; }

        public Asset Asset { get; internal set; }

        public IList<Buffer> Buffers { get; internal set; }

        public IList<BufferView> BufferViews { get; internal set; }

        public IList<Mesh> Meshes { get; internal set; }

        public IList<Node> Nodes { get; internal set; }

        [JsonPropertyName("scene")] public int? SceneReference => Scenes?.NullableIndexOf(Scene);

        public IList<Scene> Scenes { get; internal set; }
    }
}