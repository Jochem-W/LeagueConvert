using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json
{
    public class GltfAsset : IAsyncDisposable
    {
        public GltfAsset()
        {
            this.CreateAsset();
        }

        public IList<Accessor> Accessors { get; internal set; }

        public IList<Animation> Animations { get; internal set; }

        public Asset Asset { get; internal set; }

        public IList<Buffer> Buffers { get; internal set; }

        public IList<BufferView> BufferViews { get; internal set; }

        public IList<Image> Images { get; internal set; }

        public IList<Material> Materials { get; internal set; }

        public IList<Mesh> Meshes { get; internal set; }

        public IList<Node> Nodes { get; internal set; }

        public IList<Sampler> Samplers { get; internal set; }

        [JsonIgnore] public Scene Scene { get; set; }

        [JsonPropertyName("scene")] public int? SceneReference => Scene != null ? Scenes.IndexOf(Scene) : null;

        public IList<Scene> Scenes { get; internal set; }

        public IList<Skin> Skins { get; internal set; }

        public IList<Texture> Textures { get; internal set; }

        public async ValueTask DisposeAsync()
        {
            if (Accessors == null)
                return;
            foreach (var accessor in Accessors)
                await accessor.DisposeAsync();
        }
    }
}