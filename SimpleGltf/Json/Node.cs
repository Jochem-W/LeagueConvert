using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Node
    {
        private readonly GltfAsset _gltfAsset;
        internal readonly int Index;
        private Quaternion _rotation;
        private Vector3 _scale;
        private Vector3 _translation;
        private Matrix4x4 _trs;

        private Node(GltfAsset gltfAsset, string name)
        {
            _gltfAsset = gltfAsset;
            _gltfAsset.Nodes ??= new List<Node>();
            Index = _gltfAsset.Nodes.Count;
            _gltfAsset.Nodes.Add(this);
            Name = name;
        }

        internal Node(GltfAsset gltfAsset, Quaternion rotation, Vector3 scale, Vector3 translation, string name) : this(
            gltfAsset, name)
        {
            _rotation = rotation;
            _scale = scale;
            _translation = translation;
            CalculateMatrix();
        }

        internal Node(GltfAsset gltfAsset, Matrix4x4 transform, string name) : this(gltfAsset, name)
        {
            _trs = transform;
            DecomposeTRS();
        }

        [JsonIgnore] public IList<Node> Children { get; set; }

        [JsonPropertyName("children")]
        public IEnumerable<int> ChildrenIndices => Children?.Select(child => child.Index);

        [JsonConverter(typeof(Matrix4x4Converter))]
        public Matrix4x4? Matrix
        {
            get
            {
                if (_gltfAsset.Animations != null && _gltfAsset.Animations.Any(
                    animation => animation.Channels.Any(channel => channel.Target.Node == this)))
                    return null;
                return Skin != null ? null : _trs == Matrix4x4.Identity ? null : _trs;
            }
            set
            {
                if (value == null)
                {
                    _trs = Matrix4x4.Identity;
                    return;
                }

                _trs = value.Value;
                DecomposeTRS();
            }
        }

        [JsonIgnore] public Mesh Mesh { get; set; }

        [JsonPropertyName("mesh")] public int? MeshIndex => Mesh?.Index;

        [JsonIgnore] public Skin Skin { get; set; }

        [JsonPropertyName("skin")] public int? SkinIndex => Skin?.Index;

        [JsonConverter(typeof(NullableQuaternionConverter))]
        public Quaternion? Rotation
        {
            get => Matrix != null ? null : _rotation == Quaternion.Identity ? null : _rotation;
            set
            {
                _rotation = value ?? Quaternion.Identity;
                CalculateMatrix();
            }
        }

        [JsonConverter(typeof(NullableVector3Converter))]
        public Vector3? Scale
        {
            get => Matrix != null ? null : _scale == Vector3.One ? null : _scale;
            set
            {
                _scale = value ?? Vector3.One;
                CalculateMatrix();
            }
        }

        [JsonConverter(typeof(NullableVector3Converter))]
        public Vector3? Translation
        {
            get => Matrix != null ? null : _translation == Vector3.Zero ? null : _translation;
            set
            {
                _translation = value ?? Vector3.Zero;
                CalculateMatrix();
            }
        }

        //public IList<float> Weights { get; internal set; }

        public string Name { get; }

        private void CalculateMatrix()
        {
            var translationMatrix = Matrix4x4.CreateTranslation(_translation);
            var rotationMatrix = Matrix4x4.CreateFromQuaternion(_rotation);
            var scaleMatrix = Matrix4x4.CreateScale(_scale);
            _trs = translationMatrix * rotationMatrix * scaleMatrix;
        }

        private void DecomposeTRS()
        {
            Matrix4x4.Decompose(_trs, out _scale, out _rotation, out _translation);
        }
    }
}