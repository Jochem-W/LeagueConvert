using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ImageMagick;
using LeagueConvert.Enums;
using LeagueToolkit.Helpers.Cryptography;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.IO.SkeletonFile;
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
            if (skin.State.HasFlag(SkinState.SkeletonLoaded))
                FixSkeleton(skin);
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

        private static void FixSkeleton(Skin skin)
        {
            //TODO: fix NaN inverse bind matrices
            foreach (var joint in skin.Skeleton.Joints)
                if (float.IsNaN(joint.InverseBindTransform.M11))
                {
                }
        }

        public static async Task<GltfAsset> GetGltfAsset(this Skin skin)
        {
            if (!skin.State.HasFlag(SkinState.MeshLoaded))
                return null;
            Fix(skin);

            var gltfAsset = new GltfAsset();
            gltfAsset.Scene = gltfAsset.CreateScene();
            var node = gltfAsset.CreateNode();
            gltfAsset.Scene.Nodes = new List<Node> {node};
            var sampler = gltfAsset.CreateSampler(wrapS: WrappingMode.ClampToEdge, wrapT: WrappingMode.ClampToEdge);
            node.Mesh = gltfAsset.CreateMesh();
            var buffer = gltfAsset.CreateBuffer();
            var textures = new Dictionary<IMagickImage, Texture>();
            var attributesBufferView = gltfAsset.CreateBufferView(buffer, BufferViewTarget.ArrayBuffer);
            foreach (var subMesh in skin.SimpleSkin.Submeshes)
            {
                attributesBufferView.StartAccessorGroup();
                var positionAccessor = gltfAsset
                    .CreateAccessor(ComponentType.Float, AccessorType.Vec3, minMax: true)
                    .SetBufferView(attributesBufferView);
                var normalAccessor = gltfAsset.CreateAccessor(ComponentType.Float, AccessorType.Vec3)
                    .SetBufferView(attributesBufferView);
                var uvAccessor = gltfAsset.CreateAccessor(ComponentType.Float, AccessorType.Vec2)
                    .SetBufferView(attributesBufferView);
                Accessor colourAccessor = null;
                if (subMesh.Vertices.All(vertex => vertex.Color != null))
                    colourAccessor = gltfAsset.CreateAccessor(ComponentType.Float, AccessorType.Vec4)
                        .SetBufferView(attributesBufferView);

                //SKELETON
                Accessor jointsAccessor = null;
                Accessor weightsAccessor = null;
                if (skin.State.HasFlag(SkinState.SkeletonLoaded))
                {
                    jointsAccessor = gltfAsset.CreateAccessor(ComponentType.UShort, AccessorType.Vec4)
                        .SetBufferView(attributesBufferView);
                    weightsAccessor = gltfAsset.CreateAccessor(ComponentType.Float, AccessorType.Vec4)
                        .SetBufferView(attributesBufferView);
                }

                var indicesBufferView = gltfAsset.CreateBufferView(buffer, BufferViewTarget.ElementArrayBuffer);
                indicesBufferView.StartAccessorGroup();
                var indicesAccessor = gltfAsset.CreateAccessor(ComponentType.UShort, AccessorType.Scalar)
                    .SetBufferView(indicesBufferView);
                var primitive = node.Mesh.CreatePrimitive();
                primitive.Indices = indicesAccessor;
                foreach (var index in subMesh.Indices)
                    indicesAccessor.WriteElement(index);
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
                    positionAccessor.WriteElement(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
                    normalAccessor.WriteElement(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z);
                    uvAccessor.WriteElement(vertex.UV.X, vertex.UV.Y);
                    colourAccessor?.WriteElement(vertex.Color!.Value.R, vertex.Color.Value.G,
                        vertex.Color.Value.B,
                        vertex.Color.Value.A);
                    weightsAccessor?.WriteElement(vertex.Weights[0], vertex.Weights[1], vertex.Weights[2],
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

                    jointsAccessor.WriteElement(actualJoints.Cast<dynamic>().ToArray());
                }


                //MATERIALS
                var material = gltfAsset.CreateMaterial(name: subMesh.Name);
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

                var imageBufferView = gltfAsset.CreateBufferView(buffer);
                var image = await gltfAsset.CreateImage(imageBufferView, magickImage);
                var texture = gltfAsset.CreateTexture(sampler, image);
                textures[magickImage] = texture;
                pbrMetallicRoughness.SetBaseColorTexture(texture);
            }


            //SKELETON
            if (!skin.State.HasFlag(SkinState.SkeletonLoaded))
                return gltfAsset;
            var skeletonRootNode = gltfAsset.CreateNode();
            gltfAsset.Scene.Nodes.Add(skeletonRootNode);
            var inverseBindMatricesBufferView = gltfAsset.CreateBufferView(buffer);
            inverseBindMatricesBufferView.StartAccessorGroup();
            var inverseBindMatricesAccessor = gltfAsset
                .CreateAccessor(ComponentType.Float, AccessorType.Mat4)
                .SetBufferView(inverseBindMatricesBufferView);
            var joints = new Dictionary<SkeletonJoint, Node>();
            foreach (var skeletonJoint in skin.Skeleton.Joints)
            {
                inverseBindMatricesAccessor.WriteElement(skeletonJoint.InverseBindTransform
                    .FixInverseBindMatrix()
                    .Transpose()
                    .GetValues()
                    .Cast<dynamic>()
                    .ToArray());
                var jointNode = gltfAsset.CreateNode(skeletonJoint.LocalTransform, skeletonJoint.Name);
                joints[skeletonJoint] = jointNode;
            }

            var gltfSkin = gltfAsset.CreateSkin();
            node.Skin = gltfSkin;
            gltfSkin.InverseBindMatrices = inverseBindMatricesAccessor;
            foreach (var (skeletonJoint, jointNode) in joints)
            {
                var (_, parentNode) = joints.FirstOrDefault(pair => pair.Key.ID == skeletonJoint.ParentID);
                parentNode?.AddChild(jointNode);
                if (skeletonJoint.ParentID == -1)
                    skeletonRootNode.AddChild(jointNode);
                gltfSkin.Joints.Add(jointNode);
            }

            if (!skin.State.HasFlag(SkinState.AnimationsLoaded))
                return gltfAsset;


            //ANIMATIONS
            var inputBufferView = gltfAsset.CreateBufferView(buffer);
            var trackBufferView = gltfAsset.CreateBufferView(buffer);
            foreach (var (name, animation) in skin.Animations)
            {
                var gltfAnimation = gltfAsset.CreateAnimation(name);

                inputBufferView.StartAccessorGroup();
                var input = gltfAsset
                    .CreateAccessor(ComponentType.Float, AccessorType.Scalar, minMax: true)
                    .SetBufferView(inputBufferView);
                foreach (var time in animation.Tracks[0].Rotations.Select(pair => pair.Key))
                    input.WriteElement(time);

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
                    if (tracksWithSameId.Count > jointsWithSameId.Count)
                        continue;
                    var trackPositionAmongSameId = tracksWithSameId.IndexOf(track);
                    var mostLikelyJoint = jointsWithSameId[trackPositionAmongSameId];


                    trackBufferView.StartAccessorGroup();


                    var translationOutputAccessor = gltfAsset
                        .CreateAccessor(ComponentType.Float, AccessorType.Vec3)
                        .SetBufferView(trackBufferView);
                    var translationSampler = gltfAnimation.CreateSampler(input, translationOutputAccessor);
                    var translationTarget = new AnimationTarget(mostLikelyJoint, AnimationPath.Translation);
                    gltfAnimation.CreateChannel(translationSampler, translationTarget);
                    foreach (var translation in track.Translations.Select(pair => pair.Value))
                        translationOutputAccessor.WriteElement(translation.X, translation.Y, translation.Z);


                    var rotationOutputAccessor = gltfAsset
                        .CreateAccessor(ComponentType.Float, AccessorType.Vec4)
                        .SetBufferView(trackBufferView);
                    var rotationSampler = gltfAnimation.CreateSampler(input, rotationOutputAccessor);
                    var rotationTarget = new AnimationTarget(mostLikelyJoint, AnimationPath.Rotation);
                    gltfAnimation.CreateChannel(rotationSampler, rotationTarget);
                    foreach (var rotation in track.Rotations.Select(pair => pair.Value))
                        rotationOutputAccessor.WriteElement(rotation.X, rotation.Y, rotation.Z, rotation.W);


                    var scaleOutputAccessor = gltfAsset
                        .CreateAccessor(ComponentType.Float, AccessorType.Vec3)
                        .SetBufferView(trackBufferView);
                    var scaleSampler = gltfAnimation.CreateSampler(input, scaleOutputAccessor);
                    var scaleTarget = new AnimationTarget(mostLikelyJoint, AnimationPath.Scale);
                    gltfAnimation.CreateChannel(scaleSampler, scaleTarget);
                    foreach (var scale in track.Scales.Select(pair => pair.Value))
                        scaleOutputAccessor.WriteElement(scale.X, scale.Y, scale.Z);
                }
            }

            return gltfAsset;
        }
    }
}