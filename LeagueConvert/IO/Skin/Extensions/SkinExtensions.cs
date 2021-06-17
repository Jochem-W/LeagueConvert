using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ImageMagick;
using LeagueConvert.Enums;
using LeagueToolkit.Helpers.Cryptography;
using LeagueToolkit.IO.SimpleSkinFile;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;
using SimpleGltf.Json;
using SimpleGltf.Json.Extensions;

namespace LeagueConvert.IO.Skin.Extensions
{
    public static class SkinExtensions
    {
        private static void Fix(Skin skin)
        {
            if (skin.State.HasFlag(SkinState.MeshLoaded))
                FixMesh(skin);
        }

        private static void FixMesh(Skin skin)
        {
            foreach (var vertex in skin.SimpleSkin.Submeshes.SelectMany(subMesh => subMesh.Vertices))
            {
                FixNormal(vertex);
                FixUv(vertex);
            }
        }

        private static void FixNormal(SimpleSkinVertex vertex)
        {
            var length = vertex.Normal.Length();
            if (length is 0 or float.NaN)
            {
                vertex.Normal = vertex.Position;
                length = vertex.Normal.Length();
            }

            if (length is not 1f)
                vertex.Normal = Vector3.Normalize(vertex.Normal);
        }

        private static void FixUv(SimpleSkinVertex vertex)
        {
            var x = vertex.UV.X;
            var y = vertex.UV.Y;
            if (float.IsPositiveInfinity(vertex.UV.X))
                x = float.MaxValue;
            else if (float.IsNegativeInfinity(vertex.UV.X))
                x = float.MinValue;
            if (float.IsPositiveInfinity(vertex.UV.Y))
                y = float.MaxValue;
            else if (float.IsNegativeInfinity(vertex.UV.Y))
                y = float.MinValue;
            vertex.UV = new Vector2(x, y);
        }

        public static async Task<GltfAsset> GetGltfAsset(this Skin skin)
        {
            if (!skin.State.HasFlag(SkinState.MeshLoaded))
                return null;
            Fix(skin);

            var gltfAsset = new GltfAsset();
            gltfAsset.Scene = gltfAsset.CreateScene();
            var node = gltfAsset.CreateNode();
            node.Scale = new Vector3(-1, 1, 1);
            gltfAsset.Scene.Nodes = new List<Node> {node};
            var sampler = gltfAsset.CreateSampler(WrappingMode.ClampToEdge, WrappingMode.ClampToEdge);
            node.Mesh = gltfAsset.CreateMesh();
            var buffer = gltfAsset.CreateBuffer();
            var textures = new Dictionary<IMagickImage, Texture>();
            var attributesBufferView = buffer.CreateBufferView(BufferViewTarget.ArrayBuffer);
            var indicesBufferView = buffer.CreateBufferView(BufferViewTarget.ElementArrayBuffer);
            indicesBufferView.StopStride();
            foreach (var subMesh in skin.SimpleSkin.Submeshes)
            {
                var positionAccessor = attributesBufferView.CreateFloatAccessor(AccessorType.Vec3, true);
                var normalAccessor = attributesBufferView.CreateFloatAccessor(AccessorType.Vec3, true);
                var uvAccessor = attributesBufferView.CreateFloatAccessor(AccessorType.Vec2, true);
                FloatAccessor colourAccessor = null;
                if (subMesh.Vertices.All(vertex => vertex.Color != null))
                    colourAccessor = attributesBufferView.CreateFloatAccessor(AccessorType.Vec4, true);

                //SKELETON
                UShortAccessor jointsAccessor = null;
                FloatAccessor weightsAccessor = null;
                if (skin.State.HasFlag(SkinState.SkeletonLoaded))
                {
                    jointsAccessor = attributesBufferView.CreateUShortAccessor(AccessorType.Vec4, true);
                    weightsAccessor = attributesBufferView.CreateFloatAccessor(AccessorType.Vec4, true);
                }

                var indicesAccessor = indicesBufferView.CreateUShortAccessor(AccessorType.Scalar);
                var primitive = node.Mesh.CreatePrimitive();
                primitive.Indices = indicesAccessor;
                foreach (var index in subMesh.Indices)
                    indicesAccessor.Write(index);
                primitive.SetAttribute("POSITION", positionAccessor);
                primitive.SetAttribute("NORMAL", normalAccessor);
                primitive.SetAttribute("TEXCOORD_0", uvAccessor);


                //SKELETON
                if (skin.State.HasFlag(SkinState.SkeletonLoaded))
                {
                    primitive.SetAttribute("WEIGHTS_0", weightsAccessor);
                    primitive.SetAttribute("JOINTS_0", jointsAccessor);
                }

                if (colourAccessor != null)
                    primitive.SetAttribute("COLOR_0", colourAccessor);
                foreach (var vertex in subMesh.Vertices)
                {
                    positionAccessor.Write(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
                    normalAccessor.Write(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z);
                    uvAccessor.Write(vertex.UV.X, vertex.UV.Y);
                    colourAccessor?.Write(vertex.Color!.Value.R, vertex.Color.Value.G,
                        vertex.Color.Value.B,
                        vertex.Color.Value.A);
                    weightsAccessor?.Write(vertex.Weights[0], vertex.Weights[1], vertex.Weights[2],
                        vertex.Weights[3]);
                    if (!skin.State.HasFlag(SkinState.SkeletonLoaded))
                        continue;


                    //SKELETON
                    var actualJoints = new List<ushort>();
                    for (var i = 0; i < 4; i++)
                    {
                        if (vertex.Weights[i] == 0)
                        {
                            actualJoints.Add(0);
                            continue;
                        }

                        actualJoints.Add((ushort) skin.Skeleton.Influences[vertex.BoneIndices[i]]);
                    }

                    jointsAccessor.Write(actualJoints.ToArray());
                }

                attributesBufferView.StopStride();


                //MATERIALS
                var material = gltfAsset.CreateMaterial(subMesh.Name);
                material.SetUnlit();
                primitive.Material = material;
                if (!skin.State.HasFlag(SkinState.TexturesLoaded))
                    continue;
                if (!skin.Textures.ContainsKey(subMesh.Name))
                    continue;
                var pbrMetallicRoughness = material.CreatePbrMetallicRoughness();
                var magickImage = skin.Textures[subMesh.Name];
                if (textures.ContainsKey(magickImage))
                {
                    pbrMetallicRoughness.SetBaseColorTexture(textures[magickImage]);
                    continue;
                }

                var imageBufferView = buffer.CreateBufferView();
                imageBufferView.StopStride();
                var image = await gltfAsset.CreateImage(imageBufferView, magickImage);
                var texture = gltfAsset.CreateTexture(sampler, image);
                textures[magickImage] = texture;
                pbrMetallicRoughness.SetBaseColorTexture(texture);
            }


            //SKELETON
            if (!skin.State.HasFlag(SkinState.SkeletonLoaded))
                return gltfAsset;
            var skeletonRootNode = gltfAsset.CreateNode();
            skeletonRootNode.Scale = new Vector3(-1, 1, 1);
            gltfAsset.Scene.Nodes.Add(skeletonRootNode);
            var inverseBindMatricesBufferView = buffer.CreateBufferView();
            inverseBindMatricesBufferView.StopStride();
            var inverseBindMatricesAccessor = inverseBindMatricesBufferView.CreateFloatAccessor(AccessorType.Mat4);
            var joints = new Dictionary<Joint, Node>();
            foreach (var skeletonJoint in skin.Skeleton.Joints)
            {
                inverseBindMatricesAccessor.Write(skeletonJoint.InverseBindTransform
                    .Transpose()
                    .FixInverseBindMatrix()
                    .GetValues()
                    .ToArray());
                var jointNode = gltfAsset.CreateNode(skeletonJoint.LocalTransform.Transpose(), skeletonJoint.Name);
                joints[skeletonJoint] = jointNode;
            }

            var gltfSkin = gltfAsset.CreateSkin();
            node.Skin = gltfSkin;
            gltfSkin.InverseBindMatrices = inverseBindMatricesAccessor;
            foreach (var (skeletonJoint, jointNode) in joints)
            {
                var (_, parentNode) = joints.FirstOrDefault(pair => pair.Key.Id == skeletonJoint.ParentId);
                parentNode?.AddChild(jointNode);
                if (skeletonJoint.ParentId == -1)
                    skeletonRootNode.AddChild(jointNode);
                gltfSkin.Joints.Add(jointNode);
            }

            if (!skin.State.HasFlag(SkinState.AnimationsLoaded))
                return gltfAsset;


            var translationBufferView = buffer.CreateBufferView();
            var rotationBufferView = buffer.CreateBufferView();
            var scaleBufferView = buffer.CreateBufferView();

            //ANIMATIONS
            foreach (var (name, animation) in skin.Animations)
            {
                var gltfAnimation = gltfAsset.CreateAnimation(name);
                var jointsAndHash = gltfSkin.Joints
                    .Select(joint => new KeyValuePair<Node, uint>(joint, Cryptography.ElfHash(joint.Name)))
                    .ToList();

                foreach (var track in animation.Tracks)
                {
                    var tracksWithSameId = animation.Tracks
                        .Where(t => t.JointHash == track.JointHash)
                        .ToList();
                    var jointsWithSameId = jointsAndHash
                        .Where(pair => pair.Value == track.JointHash)
                        .Select(pair => pair.Key)
                        .ToList();
                    if (tracksWithSameId.Count > jointsWithSameId.Count || jointsWithSameId.Count == 0)
                        continue;
                    var trackPositionAmongSameId = tracksWithSameId.IndexOf(track);
                    var mostLikelyJoint = jointsWithSameId[trackPositionAmongSameId];
                    if (track.Translations.Count != 0)
                    {
                        var translationInputAccessor =
                            translationBufferView.CreateFloatAccessor(AccessorType.Scalar, true);
                        var translationOutputAccessor = translationBufferView.CreateFloatAccessor(AccessorType.Vec3);
                        var translationSampler =
                            gltfAnimation.CreateSampler(translationInputAccessor, translationOutputAccessor);
                        var translationTarget = new AnimationTarget(AnimationPath.Translation) {Node = mostLikelyJoint};
                        gltfAnimation.CreateChannel(translationSampler, translationTarget);
                        foreach (var (time, translation) in track.Translations.OrderBy(pair => pair.Key))
                        {
                            translationOutputAccessor.Write(translation.X, translation.Y, translation.Z);
                            translationInputAccessor.Write(time);
                        }

                        translationBufferView.StopStride();
                    }

                    if (track.Rotations.Count != 0)
                    {
                        var rotationInputAccessor = rotationBufferView.CreateFloatAccessor(AccessorType.Scalar, true);
                        var rotationOutputAccessor = rotationBufferView.CreateFloatAccessor(AccessorType.Vec4);
                        var rotationSampler = gltfAnimation.CreateSampler(rotationInputAccessor, rotationOutputAccessor);
                        var rotationTarget = new AnimationTarget(AnimationPath.Rotation) {Node = mostLikelyJoint};
                        gltfAnimation.CreateChannel(rotationSampler, rotationTarget);
                        foreach (var (time, rotation) in track.Rotations.OrderBy(pair => pair.Key))
                        {
                            var normalized = rotation.Length() is 1f ? rotation : Quaternion.Normalize(rotation);
                            rotationOutputAccessor.Write(normalized.X, normalized.Y, normalized.Z, normalized.W);
                            rotationInputAccessor.Write(time);
                        }

                        rotationBufferView.StopStride();
                    }

                    if (track.Scales.Count != 0)
                    {
                        var scaleInputAccessor = scaleBufferView.CreateFloatAccessor(AccessorType.Scalar, true);
                        var scaleOutputAccessor = scaleBufferView.CreateFloatAccessor(AccessorType.Vec3);
                        var scaleSampler = gltfAnimation.CreateSampler(scaleInputAccessor, scaleOutputAccessor);
                        var scaleTarget = new AnimationTarget(AnimationPath.Scale) {Node = mostLikelyJoint};
                        gltfAnimation.CreateChannel(scaleSampler, scaleTarget);
                        foreach (var (time, scale) in track.Scales.OrderBy(pair => pair.Key))
                        {
                            scaleOutputAccessor.Write(scale.X, scale.Y, scale.Z);
                            scaleInputAccessor.Write(time);
                        }

                        scaleBufferView.StopStride();
                    }
                }
            }

            return gltfAsset;
        }
    }
}