using System.Numerics;
using ImageMagick;
using LeagueConvert.Enums;
using LeagueToolkit.IO.AnimationFile;
using LeagueToolkit.IO.SkeletonFile;
using Serilog;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;
using SimpleGltf.Json;
using SimpleGltf.Json.Extensions;
using Animation = SimpleGltf.Json.Animation;

namespace LeagueConvert.IO.Skin.Extensions;

public static class SkinExtensions
{
    public static async Task<GltfAsset> GetGltfAsset(this Skin skin, bool forceScale, ILogger logger = null)
    {
        if (!skin.State.HasFlagFast(SkinState.MeshLoaded))
            return null;

        var gltfAsset = new GltfAsset();
        gltfAsset.Scene = gltfAsset.CreateScene();
        var node = gltfAsset.CreateNode();
        gltfAsset.Scene.AddNode(node);
        var sampler = gltfAsset.CreateSampler(WrappingMode.ClampToEdge, WrappingMode.ClampToEdge);
        node.Mesh = gltfAsset.CreateMesh();
        var buffer = gltfAsset.CreateBuffer();
        var textures = new Dictionary<IMagickImage, Texture>();
        var attributesBufferView = gltfAsset.CreateBufferView(buffer, BufferViewTarget.ArrayBuffer);
        var indicesBufferView = gltfAsset.CreateBufferView(buffer, BufferViewTarget.ElementArrayBuffer);
        indicesBufferView.StopStride();
        foreach (var subMesh in skin.SimpleSkin.SubMeshes)
        {
            var positionAccessor = gltfAsset.CreateFloatAccessor(attributesBufferView, AccessorType.Vec3, true);
            var normalAccessor = gltfAsset.CreateFloatAccessor(attributesBufferView, AccessorType.Vec3, true);
            var uvAccessor = gltfAsset.CreateFloatAccessor(attributesBufferView, AccessorType.Vec2, true);
            // FloatAccessor colourAccessor = null;
            // if (subMesh.Vertices.All(vertex => vertex.Color != null))
            //     colourAccessor = gltfAsset.CreateFloatAccessor(attributesBufferView, AccessorType.Vec4, true);

            //SKELETON
            UShortAccessor jointsAccessor = null;
            FloatAccessor weightsAccessor = null;
            if (skin.State.HasFlagFast(SkinState.SkeletonLoaded))
            {
                jointsAccessor = gltfAsset.CreateUShortAccessor(attributesBufferView, AccessorType.Vec4, true);
                weightsAccessor = gltfAsset.CreateFloatAccessor(attributesBufferView, AccessorType.Vec4, true);
            }

            var indicesAccessor = gltfAsset.CreateUShortAccessor(indicesBufferView, AccessorType.Scalar);
            var primitive = node.Mesh.CreatePrimitive();
            primitive.Indices = indicesAccessor;
            foreach (var index in subMesh.Indices)
                indicesAccessor.Write(index);
            primitive.SetAttribute("POSITION", positionAccessor);
            primitive.SetAttribute("NORMAL", normalAccessor);
            primitive.SetAttribute("TEXCOORD_0", uvAccessor);


            //SKELETON
            if (skin.State.HasFlagFast(SkinState.SkeletonLoaded))
            {
                primitive.SetAttribute("WEIGHTS_0", weightsAccessor);
                primitive.SetAttribute("JOINTS_0", jointsAccessor);
            }

            // if (colourAccessor != null)
            //     primitive.SetAttribute("COLOR_0", colourAccessor);
            foreach (var vertex in subMesh.Vertices)
            {
                positionAccessor.Write(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
                normalAccessor.Write(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z);
                uvAccessor.Write(vertex.Uv.X, vertex.Uv.Y);
                // colourAccessor?.Write(vertex.Color!.Value.R, vertex.Color.Value.G,
                //     vertex.Color.Value.B,
                //     vertex.Color.Value.A);
                weightsAccessor?.Write(vertex.Weights[0], vertex.Weights[1], vertex.Weights[2],
                    vertex.Weights[3]);
                if (!skin.State.HasFlagFast(SkinState.SkeletonLoaded))
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
            primitive.Material = material;
            if (!skin.State.HasFlagFast(SkinState.TexturesLoaded))
                continue;

            // TODO: ignore case?
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
            imageBufferView.StopStride();
            var image = await gltfAsset.CreateImage(imageBufferView, magickImage);
            var texture = gltfAsset.CreateTexture(sampler, image);
            textures[magickImage] = texture;
            pbrMetallicRoughness.SetBaseColorTexture(texture);
        }


        //SKELETON
        if (!skin.State.HasFlagFast(SkinState.SkeletonLoaded))
        {
            node.Scale = new Vector3(-1, 1, 1);
            return gltfAsset;
        }
        
        var skeletonRootNode = gltfAsset.CreateNode();
        if (forceScale)
        {
            node.Scale = new Vector3(-1, 1, 1);
            skeletonRootNode.Scale = new Vector3(-1, 1, 1);
        }
        
        gltfAsset.Scene.AddNode(skeletonRootNode);
        var inverseBindMatricesBufferView = gltfAsset.CreateBufferView(buffer);
        inverseBindMatricesBufferView.StopStride();
        var inverseBindMatricesAccessor =
            gltfAsset.CreateFloatAccessor(inverseBindMatricesBufferView, AccessorType.Mat4);
        var joints = new Dictionary<SkeletonJoint, Node>();
        foreach (var skeletonJoint in skin.Skeleton.Joints)
        {
            inverseBindMatricesAccessor.Write(skeletonJoint.InverseBindTransform
                .Transpose()
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
            gltfSkin.AddJoint(jointNode);
        }

        if (!skin.State.HasFlagFast(SkinState.AnimationsLoaded))
            return gltfAsset;


        var translationBufferView = gltfAsset.CreateBufferView(buffer);
        var rotationBufferView = gltfAsset.CreateBufferView(buffer);
        var scaleBufferView = gltfAsset.CreateBufferView(buffer);

        //ANIMATIONS
        foreach (var (name, animation) in skin.Animations)
        {
            var gltfAnimation = gltfAsset.CreateAnimation(name);

            var mapping = skin.Skeleton.MapTracksToJoints(animation);
            foreach (var (track, skeletonJoint) in mapping)
                ApplyTrack(gltfAsset, translationBufferView, rotationBufferView, scaleBufferView, gltfAnimation, track,
                    joints[skeletonJoint]);
        }

        return gltfAsset;
    }

    private static void ApplyTrack(GltfAsset gltfAsset, BufferView translationBufferView,
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