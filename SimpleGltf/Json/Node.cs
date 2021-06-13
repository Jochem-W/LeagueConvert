using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using SimpleGltf.Extensions;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Node : IIndexable
    {
        private readonly Quaternion _defaultRotation = Quaternion.Identity;
        private readonly Vector3 _defaultScale = Vector3.Zero;
        private readonly Vector3 _defaultTranslation = Vector3.Zero;
        private readonly GltfAsset _gltfAsset;
        private Matrix4x4 _matrix;
        private Quaternion _rotation;
        private Vector3 _scale;
        private Vector3 _translation;
        internal bool Animated;

        private Node(GltfAsset gltfAsset, string name)
        {
            _gltfAsset = gltfAsset;
            _gltfAsset.Nodes ??= new List<Node>();
            Index = _gltfAsset.Nodes.Count;
            _gltfAsset.Nodes.Add(this);
            Name = name;
            Animated = false;
            _translation = _defaultTranslation;
            _rotation = _defaultRotation;
            _scale = _defaultScale;
        }

        internal Node(GltfAsset gltfAsset, Matrix4x4 transform, string name) : this(gltfAsset, name)
        {
            _matrix = transform;
            DecomposeMatrix();
            CalculateMatrix();
        }

        [JsonConverter(typeof(EnumerableIndexableConverter<Node>))]
        public IList<Node> Children { get; set; }

        [JsonConverter(typeof(Matrix4x4Converter))]
        public Matrix4x4? Matrix
        {
            get
            {
                if (_matrix == Matrix4x4.Identity || Animated)
                    return null;
                return _matrix;
            }
            set
            {
                _matrix = value ?? Matrix4x4.Identity;
                DecomposeMatrix();
            }
        }

        [JsonConverter(typeof(IndexableConverter<Mesh>))]
        public Mesh Mesh { get; set; }

        [JsonConverter(typeof(IndexableConverter<Skin>))]
        public Skin Skin { get; set; }

        [JsonConverter(typeof(NullableQuaternionConverter))]
        public Quaternion? Rotation
        {
            get => Matrix == null ? _rotation != _defaultRotation ? _rotation : null : null;
            set
            {
                _rotation = value.HasValue ? Quaternion.Normalize(value.Value) : _defaultRotation;
                CalculateMatrix();
            }
        }

        [JsonConverter(typeof(NullableVector3Converter))]
        public Vector3? Scale
        {
            get => Matrix == null ? _scale != _defaultScale ? _scale : null : null;
            set
            {
                _scale = value ?? _defaultScale;
                CalculateMatrix();
            }
        }

        [JsonConverter(typeof(NullableVector3Converter))]
        public Vector3? Translation
        {
            get => Matrix == null ? _translation != _defaultTranslation ? _translation : null : null;
            set
            {
                _translation = value ?? _defaultTranslation;
                CalculateMatrix();
            }
        }

        public string Name { get; }

        [JsonIgnore] public int Index { get; }

        private void CalculateMatrix()
        {
            _matrix = Matrix4x4.CreateTranslation(_translation).Transpose() *
                      Matrix4x4.CreateFromQuaternion(_rotation).Transpose() * Matrix4x4.CreateScale(_scale);
        }

        private void DecomposeMatrix()
        {
            Matrix4x4.Decompose(_matrix.Transpose(), out var scale, out var rotation, out var translation);
            Scale = scale;
            Rotation = rotation;
            Translation = translation;
        }
    }
}