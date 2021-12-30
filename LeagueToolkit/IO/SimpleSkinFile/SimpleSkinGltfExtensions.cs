using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ImageMagick;
using LeagueToolkit.Helpers.Cryptography;
using LeagueToolkit.IO.SkeletonFile;
using SharpGLTF.Animations;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using GltfAnimation = SharpGLTF.Schema2.Animation;
using LeagueAnimation = LeagueToolkit.IO.AnimationFile.Animation;

namespace LeagueToolkit.IO.SimpleSkinFile;

using VERTEX = VertexBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>;
using VERTEX_SKINNED = VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>;

public static class SimpleSkinGltfExtensions
{
    public static ModelRoot ToGltf(this SimpleSkin skn, Dictionary<string, MagickImage> materialTextues = null)
    {
        var sceneBuilder = new SceneBuilder("model");
        var meshBuilder = VERTEX.CreateCompatibleMesh();

        foreach (var submesh in skn.Submeshes)
        {
            var material = new MaterialBuilder(submesh.Name).WithUnlitShader();
            var submeshPrimitive = meshBuilder.UsePrimitive(material);

            // Assign submesh Image
            if (materialTextues is not null && materialTextues.ContainsKey(submesh.Name))
            {
                var submeshImage = materialTextues[submesh.Name];
                AssignMaterialTexture(material, submeshImage);
            }

            // Build vertices
            var vertices = new List<VERTEX>(submesh.Vertices.Count);
            foreach (var vertex in submesh.Vertices)
            {
                var positionNormal = new VertexPositionNormal(vertex.Position, vertex.Normal);
                var uv = new VertexTexture1(vertex.UV);

                vertices.Add(new VERTEX(positionNormal, uv));
            }

            // Add vertices to primitive
            for (var i = 0; i < submesh.Indices.Count; i += 3)
            {
                var v1 = vertices[submesh.Indices[i + 0]];
                var v2 = vertices[submesh.Indices[i + 1]];
                var v3 = vertices[submesh.Indices[i + 2]];

                submeshPrimitive.AddTriangle(v1, v2, v3);
            }
        }

        sceneBuilder.AddRigidMesh(meshBuilder,
            new AffineTransform(new Vector3(-1, 1, 1), Quaternion.Identity, Vector3.Zero).Matrix);

        return sceneBuilder.ToGltf2();
    }

    public static ModelRoot ToGltf(this SimpleSkin skn, Skeleton skeleton,
        Dictionary<string, MagickImage> materialTextues = null, List<(string, LeagueAnimation)> leagueAnimations = null)
    {
        var sceneBuilder = new SceneBuilder();
        var rootNodeBuilder = new NodeBuilder("model");

        var meshBuilder = VERTEX_SKINNED.CreateCompatibleMesh();
        var bones = CreateSkeleton(rootNodeBuilder, skeleton);

        // Build mesh primitives
        foreach (var submesh in skn.Submeshes)
        {
            var material = new MaterialBuilder(submesh.Name).WithUnlitShader();
            var submeshPrimitive = meshBuilder.UsePrimitive(material);

            // Assign submesh Image
            if (materialTextues is not null && materialTextues.ContainsKey(submesh.Name))
            {
                var submeshImage = materialTextues[submesh.Name];
                AssignMaterialTexture(material, submeshImage);
            }

            // Build vertices
            var vertices = new List<VERTEX_SKINNED>(submesh.Vertices.Count);
            foreach (var vertex in submesh.Vertices)
            {
                var positionNormal = new VertexPositionNormal(vertex.Position, vertex.Normal);
                var uv = new VertexTexture1(vertex.UV);
                var joints = new VertexJoints4((skeleton.Influences[vertex.BoneIndices[0]], vertex.Weights[0]),
                    (skeleton.Influences[vertex.BoneIndices[1]], vertex.Weights[1]),
                    (skeleton.Influences[vertex.BoneIndices[2]], vertex.Weights[2]),
                    (skeleton.Influences[vertex.BoneIndices[3]], vertex.Weights[3]));

                vertices.Add(new VERTEX_SKINNED(positionNormal, uv, joints));
            }

            // Add vertices to primitive
            for (var i = 0; i < submesh.Indices.Count; i += 3)
            {
                var a = vertices[submesh.Indices[i + 0]];
                var b = vertices[submesh.Indices[i + 1]];
                var c = vertices[submesh.Indices[i + 2]];

                submeshPrimitive.AddTriangle(c, b, a);
            }
        }

        // Add mesh to scene
        sceneBuilder.AddSkinnedMesh(meshBuilder, bones.ToArray());

        // Create animations
        if (leagueAnimations is not null) CreateAnimations(bones.Select(x => x.Node).ToList(), leagueAnimations);

        // Flip the scene across the X axis
        sceneBuilder.ApplyBasisTransform(Matrix4x4.CreateScale(new Vector3(-1, 1, 1)));

        return sceneBuilder.ToGltf2();
    }

    private static void AssignMaterialTexture(MaterialBuilder materialBuilder, MagickImage texture)
    {
        var textureStream = new MemoryStream();

        texture.Write(textureStream, MagickFormat.Png);

        materialBuilder
            .UseChannel(KnownChannel.BaseColor)
            .UseTexture()
            .WithPrimaryImage(new MemoryImage(textureStream.GetBuffer()));
    }

    private static List<(NodeBuilder Node, Matrix4x4 InverseBindMatrix)> CreateSkeleton(NodeBuilder rootNode,
        Skeleton skeleton)
    {
        var bones = new List<(NodeBuilder, Matrix4x4)>();

        foreach (var joint in skeleton.Joints)
            // Root
            if (joint.ParentID == -1)
            {
                var jointNode = rootNode.CreateNode(joint.Name);

                jointNode.LocalTransform = joint.LocalTransform;

                bones.Add((jointNode, joint.InverseBindTransform));
            }
            else
            {
                var parentJoint = skeleton.Joints.FirstOrDefault(x => x.ID == joint.ParentID);
                var parentNode = bones.FirstOrDefault(x => x.Item1.Name == parentJoint.Name).Item1;
                var jointNode = parentNode.CreateNode(joint.Name);

                jointNode.LocalTransform = joint.LocalTransform;

                bones.Add((jointNode, joint.InverseBindTransform));
            }

        return bones;
    }

    private static void CreateAnimations(List<NodeBuilder> joints, List<(string, LeagueAnimation)> leagueAnimations)
    {
        // Check if all animations have names, if not then create them
        for (var i = 0; i < leagueAnimations.Count; i++)
            if (string.IsNullOrEmpty(leagueAnimations[i].Item1))
                leagueAnimations[i] = ("Animation" + i, leagueAnimations[i].Item2);
        
        var jointsByElfHash = joints
            .Select(joint => new KeyValuePair<uint, NodeBuilder>(Cryptography.ElfHash(joint.Name), joint))
            .ToList();
        
        foreach (var (animationName, leagueAnimation) in leagueAnimations)
        foreach (var track in leagueAnimation.Tracks)
        {
            // wanted to rewrite this but I couldn't get it to work
            // pretty sure the LINQ usage is terrible for performance
            var applicableJoints = jointsByElfHash
                        .Where(pair => pair.Key == track.JointHash)
                        .ToList();
                    NodeBuilder joint;
                    switch (applicableJoints.Count)
                    {
                        case 0: // no joints with the same hash
                            continue;
                        case 1: // one joint with the same hash, use that one
                            joint = applicableJoints[0].Value;
                            break;
                        default:
                            var tracksWithSameHash = leagueAnimation.Tracks
                                .Where(t => t.JointHash == track.JointHash)
                                .ToList();
                            var currentTrackPosition = tracksWithSameHash.IndexOf(track);
                            // same amount of joints and tracks with the same hash
                            // use the position of the current track among all tracks with the same hash as index
                            // assuming that the order of joints in the anm matches the order of joints in the skl
                            if (applicableJoints.Count == tracksWithSameHash.Count)
                            {
                                joint = applicableJoints[currentTrackPosition].Value;
                                break;
                            }
                            // different amount of joints and tracks with the same hash
                            // instead of guessing, move to the next track
                            continue;
                    }

                    if (track.Translations.Count == 0) track.Translations.Add(0.0f, new Vector3(0, 0, 0));
                    if (track.Translations.Count == 1) track.Translations.Add(1.0f, new Vector3(0, 0, 0));
                    CurveBuilder<Vector3> translationBuilder = joint.UseTranslation().UseTrackBuilder(animationName);
                    foreach (var translation in track.Translations)
                    {
                        translationBuilder.SetPoint(translation.Key, translation.Value);
                    }

                    if (track.Rotations.Count == 0) track.Rotations.Add(0.0f, Quaternion.Identity);
                    if (track.Rotations.Count == 1) track.Rotations.Add(1.0f, Quaternion.Identity);
                    CurveBuilder<Quaternion> rotationBuilder = joint.UseRotation().UseTrackBuilder(animationName);
                    foreach (var rotation in track.Rotations)
                    {
                        rotationBuilder.SetPoint(rotation.Key, rotation.Value);
                    }

                    if (track.Scales.Count == 0) track.Scales.Add(0.0f, new Vector3(1, 1, 1));
                    if (track.Scales.Count == 1) track.Scales.Add(1.0f, new Vector3(1, 1, 1));
                    CurveBuilder<Vector3> scaleBuilder = joint.UseScale().UseTrackBuilder(animationName);
                    foreach (var scale in track.Scales.ToList())
                    {
                        scaleBuilder.SetPoint(scale.Key, scale.Value);
                    }
        }
    }

    public static (SimpleSkin, Skeleton) ToLeagueModel(this ModelRoot root)
    {
        if (root.LogicalMeshes.Count != 1)
            throw new Exception("Invalid Mesh Count: " + root.LogicalMeshes.Count + " (must be 1)");
        if (root.LogicalSkins.Count != 1)
            throw new Exception("Invalid Skin count: " + root.LogicalSkins.Count + " (must be 1)");

        var mesh = root.LogicalMeshes[0];

        List<byte> influences = new();
        List<SimpleSkinSubmesh> submeshes = new(mesh.Primitives.Count);
        foreach (var primitive in mesh.Primitives)
        {
            List<uint> indices = new(primitive.GetIndices());

            var vertexPositionAccessor = GetVertexAccessor("POSITION", primitive.VertexAccessors).AsVector3Array();
            var vertexNormalAccessor = GetVertexAccessor("NORMAL", primitive.VertexAccessors).AsVector3Array();
            var vertexUvAccessor = GetVertexAccessor("TEXCOORD_0", primitive.VertexAccessors).AsVector2Array();
            var vertexBonesAccessor = GetVertexAccessor("JOINTS_0", primitive.VertexAccessors).AsVector4Array();
            var vertexWeightsAccessor = GetVertexAccessor("WEIGHTS_0", primitive.VertexAccessors).AsVector4Array();

            List<SimpleSkinVertex> vertices = new(vertexPositionAccessor.Count);
            for (var i = 0; i < vertexPositionAccessor.Count; i++)
            {
                var bonesVector = vertexBonesAccessor[i];
                byte[] influenceBones =
                {
                    (byte) bonesVector.X,
                    (byte) bonesVector.Y,
                    (byte) bonesVector.Z,
                    (byte) bonesVector.W
                };

                for (byte b = 0; b < influenceBones.Length; b++)
                    if (!influences.Any(x => x == influenceBones[b]))
                        influences.Add(influenceBones[b]);

                var weightsVector = vertexWeightsAccessor[i];
                float[] weights =
                {
                    weightsVector.X,
                    weightsVector.Y,
                    weightsVector.Z,
                    weightsVector.W
                };

                var vertexPosition = vertexPositionAccessor[i];

                var vertex = new SimpleSkinVertex(vertexPosition, influenceBones, weights, vertexNormalAccessor[i],
                    vertexUvAccessor[i]);

                vertices.Add(vertex);
            }

            submeshes.Add(new SimpleSkinSubmesh(primitive.Material.Name, indices.Select(x => (ushort) x).ToList(),
                vertices));
        }

        var simpleSkin = new SimpleSkin(submeshes);
        var skeleton = CreateLeagueSkeleton(root.LogicalSkins[0]);

        return (simpleSkin, skeleton);
    }

    private static Skeleton CreateLeagueSkeleton(Skin skin)
    {
        List<Node> nodes = new(skin.JointsCount);
        for (var i = 0; i < skin.JointsCount; i++) nodes.Add(skin.GetJoint(i).Joint);

        List<SkeletonJoint> joints = new(nodes.Count);
        for (var i = 0; i < nodes.Count; i++)
        {
            var jointNode = nodes[i];

            // If parent is null or isn't a skin joint then the joint is a root bone
            if (jointNode.VisualParent is null || !jointNode.VisualParent.IsSkinJoint)
            {
                var scale = jointNode.LocalTransform.Scale;

                joints.Add(new SkeletonJoint((short) i, -1, jointNode.Name, jointNode.LocalTransform.Translation, scale,
                    jointNode.LocalTransform.Rotation));
            }
            else
            {
                var parentId = (short) nodes.IndexOf(jointNode.VisualParent);

                joints.Add(new SkeletonJoint((short) i, parentId, jointNode.Name, jointNode.LocalTransform.Translation,
                    jointNode.LocalTransform.Scale, jointNode.LocalTransform.Rotation));
            }
        }

        List<short> influences = new(joints.Count);
        for (short i = 0; i < joints.Count; i++) influences.Add(i);

        return new Skeleton(joints, influences);
    }

    private static Accessor GetVertexAccessor(string name, IReadOnlyDictionary<string, Accessor> vertexAccessors)
    {
        if (vertexAccessors.TryGetValue(name, out var accessor))
            return accessor;
        throw new Exception("Mesh does not contain a vertex accessor: " + name);
    }
}