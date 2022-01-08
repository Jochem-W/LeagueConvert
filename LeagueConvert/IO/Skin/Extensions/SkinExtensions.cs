using System.Numerics;
using ImageMagick;
using LeagueConvert.Enums;
using LeagueToolkit.IO.AnimationFile;
using Serilog;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;
using SimpleGltf.Json;
using SimpleGltf.Json.Extensions;
using Animation = SimpleGltf.Json.Animation;
using Buffer = SimpleGltf.Json.Buffer;

namespace LeagueConvert.IO.Skin.Extensions;

public static class SkinExtensions
{
    public static async Task<GltfAsset> GetGltfAsset(this Skin skin, bool forceScale, ILogger logger = null)
    {
        if (!skin.State.HasFlagFast(SkinState.MeshLoaded)) return null;

        var gltfAsset = new GltfAsset();
        gltfAsset.Scene = gltfAsset.CreateScene();
        var buffer = gltfAsset.CreateBuffer();

        var rootNode = await skin.CreateMeshAsync(gltfAsset, buffer);

        if (!skin.State.HasFlagFast(SkinState.SkeletonLoaded))
        {
            rootNode.Scale = new Vector3(-1, 1, 1);
            return gltfAsset;
        }

        var joints = skin.BuildSkeleton(gltfAsset, buffer, rootNode, forceScale);

        if (skin.State.HasFlagFast(SkinState.AnimationsLoaded)) skin.CreateAnimations(gltfAsset, buffer, joints);

        return gltfAsset;
    }

    private static async Task<Node> CreateMeshAsync(this Skin skin, GltfAsset gltfAsset, Buffer buffer)
    {
        var rootNode = gltfAsset.CreateNode();
        gltfAsset.Scene.AddNode(rootNode);
        rootNode.Mesh = gltfAsset.CreateMesh();
        var attributesBufferView = gltfAsset.CreateBufferView(buffer, BufferViewTarget.ArrayBuffer);
        var indicesBufferView = gltfAsset.CreateBufferView(buffer, BufferViewTarget.ElementArrayBuffer);
        indicesBufferView.StopStride();

        Sampler sampler = null;
        var textures = new Dictionary<IMagickImage, Texture>();

        foreach (var subMesh in skin.SimpleSkin.SubMeshes)
        {
            var primitive = rootNode.Mesh.CreatePrimitive();

            // Vertices
            var positionAccessor = gltfAsset.CreateFloatAccessor(attributesBufferView, AccessorType.Vec3, true);
            primitive.SetAttribute("POSITION", positionAccessor);
            var normalAccessor = gltfAsset.CreateFloatAccessor(attributesBufferView, AccessorType.Vec3, true);
            primitive.SetAttribute("NORMAL", normalAccessor);
            var uvAccessor = gltfAsset.CreateFloatAccessor(attributesBufferView, AccessorType.Vec2, true);
            primitive.SetAttribute("TEXCOORD_0", uvAccessor);
            // FloatAccessor colourAccessor = null;
            // if (skin.SimpleSkin.VertexType == SimpleSkinVertexType.Color)
            // {
            //     colourAccessor = gltfAsset.CreateFloatAccessor(attributesBufferView, AccessorType.Vec4, true);
            //     primitive.SetAttribute("COLOR_0", colourAccessor);
            // }

            UShortAccessor jointsAccessor = null;
            FloatAccessor weightsAccessor = null;
            if (skin.State.HasFlagFast(SkinState.SkeletonLoaded))
            {
                jointsAccessor = gltfAsset.CreateUShortAccessor(attributesBufferView, AccessorType.Vec4, true);
                primitive.SetAttribute("JOINTS_0", jointsAccessor);
                weightsAccessor = gltfAsset.CreateFloatAccessor(attributesBufferView, AccessorType.Vec4, true);
                primitive.SetAttribute("WEIGHTS_0", weightsAccessor);
            }

            foreach (var vertex in subMesh.Vertices)
            {
                positionAccessor.Write(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
                normalAccessor.Write(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z);
                uvAccessor.Write(vertex.Uv.X, vertex.Uv.Y);
                // colourAccessor?.Write(vertex.Color!.Value.R, vertex.Color.Value.G,
                //     vertex.Color.Value.B,
                //     vertex.Color.Value.A);

                if (jointsAccessor != null)
                {
                    var joints = new List<ushort>();
                    for (var i = 0; i < 4; i++)
                    {
                        if (vertex.Weights[i] == 0)
                        {
                            joints.Add(0);
                            continue;
                        }

                        joints.Add((ushort) skin.Skeleton.Influences[vertex.BoneIndices[i]]);
                    }

                    jointsAccessor.Write(joints.ToArray());
                }

                weightsAccessor?.Write(vertex.Weights[0], vertex.Weights[1], vertex.Weights[2],
                    vertex.Weights[3]);
            }

            attributesBufferView.StopStride();


            // Indices
            var indicesAccessor = gltfAsset.CreateUShortAccessor(indicesBufferView, AccessorType.Scalar);
            primitive.Indices = indicesAccessor;
            foreach (var index in subMesh.Indices) indicesAccessor.Write(index);
            indicesBufferView.StopStride();


            // Materials
            if (!skin.State.HasFlagFast(SkinState.TexturesLoaded)) continue;
            if (!skin.Textures.TryGetValue(subMesh.Name, out var magickImage)) continue; // TODO: lowercase?

            var material = gltfAsset.CreateMaterial(subMesh.Name);
            primitive.Material = material;
            sampler ??= gltfAsset.CreateSampler(WrappingMode.ClampToEdge, WrappingMode.ClampToEdge);

            var pbrMetallicRoughness = material.CreatePbrMetallicRoughness();
            if (!textures.TryGetValue(magickImage, out var texture))
            {
                var imageBufferView = gltfAsset.CreateBufferView(buffer);
                imageBufferView.StopStride();
                var image = await gltfAsset.CreateImage(imageBufferView, magickImage);
                texture = gltfAsset.CreateTexture(sampler, image);
                textures[magickImage] = texture;
            }

            pbrMetallicRoughness.SetBaseColorTexture(texture);
        }

        return rootNode;
    }

    private static IReadOnlyList<Node> BuildSkeleton(this Skin skin, GltfAsset gltfAsset,
        Buffer buffer, Node rootNode, bool forceScale)
    {
        var skeletonRootNode = gltfAsset.CreateNode();
        if (forceScale)
        {
            rootNode.Scale = new Vector3(-1, 1, 1);
            skeletonRootNode.Scale = new Vector3(-1, 1, 1);
        }

        gltfAsset.Scene.AddNode(skeletonRootNode);
        var inverseBindMatricesBufferView = gltfAsset.CreateBufferView(buffer);
        inverseBindMatricesBufferView.StopStride();
        var inverseBindMatricesAccessor =
            gltfAsset.CreateFloatAccessor(inverseBindMatricesBufferView, AccessorType.Mat4);

        var gltfSkin = gltfAsset.CreateSkin();
        rootNode.Skin = gltfSkin;
        var jointNodes = new Node[skin.Skeleton.Joints.Count];
        foreach (var skeletonJoint in skin.Skeleton.Joints)
        {
            inverseBindMatricesAccessor.Write(skeletonJoint.InverseBindTransform
                .Transpose()
                .GetValues()
                .ToArray());
            var jointNode = gltfAsset.CreateNode(skeletonJoint.LocalTransform.Transpose(), skeletonJoint.Name);
            jointNodes[skeletonJoint.Id] = jointNode;
            if (skeletonJoint.ParentId == -1)
                skeletonRootNode.AddChild(jointNode);
            else
                jointNodes[skeletonJoint.ParentId].AddChild(jointNode);
            gltfSkin.AddJoint(jointNode);
        }

        gltfSkin.InverseBindMatrices = inverseBindMatricesAccessor;

        return jointNodes;
    }

    private static void CreateAnimations(this Skin skin, GltfAsset gltfAsset, Buffer buffer,
        IReadOnlyList<Node> joints)
    {
        var translationBufferView = gltfAsset.CreateBufferView(buffer);
        var rotationBufferView = gltfAsset.CreateBufferView(buffer);
        var scaleBufferView = gltfAsset.CreateBufferView(buffer);

        foreach (var (name, animation) in skin.Animations)
        {
            var gltfAnimation = gltfAsset.CreateAnimation(name);

            var mapping = skin.Skeleton.MapTracksToJoints(animation);

            foreach (var (track, skeletonJoint) in mapping)
                WriteAnimationChannel(gltfAsset, translationBufferView, rotationBufferView, scaleBufferView,
                    gltfAnimation, track,
                    joints[skeletonJoint.Id]);
        }
    }

    private static void WriteAnimationChannel(GltfAsset gltfAsset, BufferView translationBufferView,
        BufferView rotationBufferView, BufferView scaleBufferView, Animation gltfAnimation,
        AnimationTrack track, Node jointNode)
    {
        if (track.Translations.Count != 0)
        {
            var translationInputAccessor =
                gltfAsset.CreateFloatAccessor(translationBufferView, AccessorType.Scalar, true);
            var translationOutputAccessor =
                gltfAsset.CreateFloatAccessor(translationBufferView, AccessorType.Vec3);
            var translationSampler =
                gltfAnimation.CreateSampler(translationInputAccessor, translationOutputAccessor);
            var translationTarget = new Target(AnimationPath.Translation) {Node = jointNode};
            gltfAnimation.CreateChannel(translationSampler, translationTarget);
            foreach (var (time, translation) in track.Translations)
            {
                translationOutputAccessor.Write(translation.X, translation.Y, translation.Z);
                translationInputAccessor.Write(time);
            }

            translationBufferView.StopStride();
        }

        if (track.Rotations.Count != 0)
        {
            var rotationInputAccessor =
                gltfAsset.CreateFloatAccessor(rotationBufferView, AccessorType.Scalar, true);
            var rotationOutputAccessor =
                gltfAsset.CreateFloatAccessor(rotationBufferView, AccessorType.Vec4);
            var rotationSampler =
                gltfAnimation.CreateSampler(rotationInputAccessor, rotationOutputAccessor);
            var rotationTarget = new Target(AnimationPath.Rotation) {Node = jointNode};
            gltfAnimation.CreateChannel(rotationSampler, rotationTarget);
            foreach (var (time, rotation) in track.Rotations)
            {
                rotationOutputAccessor.Write(rotation.X, rotation.Y, rotation.Z, rotation.W);
                rotationInputAccessor.Write(time);
            }

            rotationBufferView.StopStride();
        }

        if (track.Scales.Count != 0)
        {
            var scaleInputAccessor =
                gltfAsset.CreateFloatAccessor(scaleBufferView, AccessorType.Scalar, true);
            var scaleOutputAccessor = gltfAsset.CreateFloatAccessor(scaleBufferView, AccessorType.Vec3);
            var scaleSampler = gltfAnimation.CreateSampler(scaleInputAccessor, scaleOutputAccessor);
            var scaleTarget = new Target(AnimationPath.Scale) {Node = jointNode};
            gltfAnimation.CreateChannel(scaleSampler, scaleTarget);
            foreach (var (time, scale) in track.Scales)
            {
                scaleOutputAccessor.Write(scale.X, scale.Y, scale.Z);
                scaleInputAccessor.Write(time);
            }

            scaleBufferView.StopStride();
        }
    }
}