using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
using SimpleGltf.Extensions;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Node
    {
        private readonly GltfAsset _gltfAsset;
        private Quaternion _rotation;
        private Vector3 _scale;
        private Vector3 _translation;
        internal IList<Node> Children;

        internal Node(GltfAsset gltfAsset, Quaternion rotation, Vector3 scale, Vector3 translation, string name)
        {
            _gltfAsset = gltfAsset;
            _gltfAsset.Nodes ??= new List<Node>();
            _gltfAsset.Nodes.Add(this);
            Name = name;
            _rotation = rotation;
            _scale = scale;
            _translation = translation;
        }

        [JsonPropertyName("children")]
        public IEnumerable<int> ChildrenReferences => Children?.Select(node => _gltfAsset.Nodes.IndexOf(node));

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

        [JsonIgnore] public Mesh Mesh { get; set; }
        
        [JsonPropertyName("mesh")] public int? MeshReference => Mesh == null ? null : _gltfAsset.Meshes.IndexOf(Mesh);

        public Quaternion? Rotation
        {
            get => Matrix == null ? _rotation == Quaternion.Identity ? null : _rotation : null;
            set => _rotation = value ?? Quaternion.Identity;
        }

        public Vector3? Scale
        {
            get => Matrix == null ? _scale == Vector3.One ? null : _scale : null;
            set => _scale = value ?? Vector3.One;
        }

        public Vector3? Translation
        {
            get => Matrix == null ? _translation == Vector3.Zero ? null : _translation : null;
            set => _translation = value ?? Vector3.Zero;
        }

        //public IList<float> Weights { get; internal set; }

        public string Name { get; }
    }
}