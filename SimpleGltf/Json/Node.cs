using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
using SimpleGltf.Converters;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json
{
    internal class Node
    {
        internal readonly GltfAsset GltfAsset;
        internal IList<Node> Children;
        internal Mesh Mesh;

        internal Node(GltfAsset gltfAsset, string name)
        {
            GltfAsset = gltfAsset;
            Name = name;
            GltfAsset.Nodes ??= new List<Node>();
            GltfAsset.Nodes.Add(this);
        }

        internal Node(Scene scene, string name) : this(scene.GltfAsset, name)
        {
            scene.Nodes ??= new List<Node>();
            scene.Nodes.Add(this);
        }

        internal Node(Node parent, string name) : this(parent.GltfAsset, name)
        {
            parent.Children ??= new List<Node>();
            parent.Children.Add(this);
        }

        [JsonPropertyName("children")]
        public IEnumerable<int> ChildrenReferences => Children?.Select(node => GltfAsset.Nodes.IndexOf(node));

        [JsonConverter(typeof(Matrix4x4Converter))]
        public Matrix4x4? Matrix
        {
            get
            {
                if (Translation == null && Rotation == null && Scale == null)
                    return null;
                var translationMatrix = Matrix4x4.CreateTranslation(Translation!.Value);
                var rotationMatrix = Matrix4x4.CreateFromQuaternion(Rotation!.Value);
                var scaleMatrix = Matrix4x4.CreateScale(Scale!.Value);
                return translationMatrix * rotationMatrix * scaleMatrix;
            }
        }

        [JsonPropertyName("mesh")] public int? MeshReference => GltfAsset.Meshes?.NullableIndexOf(Mesh);

        public string Name { get; set; }

        public Quaternion? Rotation { get; set; }

        public Vector3? Scale { get; set; }

        public Vector3? Translation { get; set; }
    }
}