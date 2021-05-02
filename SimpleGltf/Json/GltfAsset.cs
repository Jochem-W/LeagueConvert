using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json
{
    internal class GltfAsset : IAsyncDisposable
    {
        internal GltfAsset()
        {
            Asset = new Asset(this);
        }

        internal Scene Scene { get; set; }

        public IList<Accessor> Accessors { get; set; }

        public Asset Asset { get; }

        public IList<Buffer> Buffers { get; set; }

        public IList<BufferView> BufferViews { get; set; }

        public IList<Mesh> Meshes { get; set; }

        public IList<Node> Nodes { get; set; }

        [JsonPropertyName("scene")] public int? SceneReference => Scenes?.NullableIndexOf(Scene);

        public IList<Scene> Scenes { get; set; }

        public async ValueTask DisposeAsync()
        {
            if (Buffers == null)
                return;
            for (var i = 0; i < Buffers.Count; i++)
            {
                await Buffers[i].DisposeAsync();
                i--;
            }
        }
    }
}