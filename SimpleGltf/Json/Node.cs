using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
using SimpleGltf.Extensions;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    internal class Node
    {
        internal readonly GltfAsset GltfAsset;
        private Quaternion _rotation;
        private Vector3 _scale;
        private Vector3 _translation;
        internal IList<Node> Children;
        internal Mesh Mesh;

        internal Node(GltfAsset gltfAsset, string name)
        {
            GltfAsset = gltfAsset;
            Name = name;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
            Translation = Vector3.Zero;
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
                //TODO: return null if node is animated
                if (_translation == Vector3.Zero && _rotation == Quaternion.Identity && _scale == Vector3.One)
                    return null;
                var translationMatrix = Matrix4x4.CreateTranslation(_translation);
                var rotationMatrix = Matrix4x4.CreateFromQuaternion(_rotation);
                var scaleMatrix = Matrix4x4.CreateScale(_scale);
                var trsMatrix = translationMatrix * rotationMatrix * scaleMatrix;
                if (trsMatrix == Matrix4x4.Identity)
                    return null;
                return trsMatrix;
            }
        }

        [JsonPropertyName("mesh")] public int? MeshReference => GltfAsset.Meshes?.NullableIndexOf(Mesh);

        public Quaternion? Rotation
        {
            get => Matrix == null ? _rotation == Quaternion.Identity ? null : _rotation : null;
            internal set => _rotation = value ?? Quaternion.Identity;
        }

        public Vector3? Scale
        {
            get => Matrix == null ? _scale == Vector3.One ? null : _scale : null;
            internal set => _scale = value ?? Vector3.One;
        }

        public Vector3? Translation
        {
            get => Matrix == null ? _translation == Vector3.Zero ? null : _translation : null;
            internal set => _translation = value ?? Vector3.Zero;
        }

        //public IList<float> Weights { get; internal set; }

        public string Name { get; }
    }
}