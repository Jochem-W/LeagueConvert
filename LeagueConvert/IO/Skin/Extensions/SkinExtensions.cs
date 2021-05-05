using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ImageMagick;
using LeagueConvert.Enums;
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
        public static async Task<GltfAsset> GetGltfAsset(this Skin skin)
        {
            if (!skin.State.HasFlag(SkinState.MeshLoaded))
                return null;

            var textures = new Dictionary<IMagickImage, Texture>();
            var gltfAsset = new GltfAsset();
            var sampler = gltfAsset.CreateSampler(wrapS: WrappingMode.ClampToEdge, wrapT: WrappingMode.ClampToEdge);
            var buffer = gltfAsset.CreateBuffer();
            var scene = gltfAsset.CreateScene();
            var node = gltfAsset.CreateNode();
            scene.AddNode(node);
            node.Mesh = gltfAsset.CreateMesh();
            foreach (var subMesh in skin.SimpleSkin.Submeshes)
            {
                //MESH
                var primitive = node.Mesh.CreatePrimitive();
                var indicesBufferView = gltfAsset.CreateBufferView(buffer, BufferViewTarget.ElementArrayBuffer);
                var indicesAccessor = gltfAsset.CreateAccessor(ComponentType.UShort, AccessorType.Scalar)
                    .SetBufferView(indicesBufferView);
                primitive.Indices = indicesAccessor;
                foreach (var index in subMesh.Indices)
                    indicesAccessor.WriteElement(index);
                var attributesBufferView = gltfAsset.CreateBufferView(buffer, BufferViewTarget.ArrayBuffer);
                var positionAccessor = gltfAsset
                    .CreateAccessor(ComponentType.Float, AccessorType.Vec3, minMax: true)
                    .SetBufferView(attributesBufferView);
                primitive.SetAttribute("POSITION", positionAccessor);
                var normalAccessor = gltfAsset.CreateAccessor(ComponentType.Float, AccessorType.Vec3)
                    .SetBufferView(attributesBufferView);
                primitive.SetAttribute("NORMAL", normalAccessor);
                var uvAccessor = gltfAsset.CreateAccessor(ComponentType.Float, AccessorType.Vec2)
                    .SetBufferView(attributesBufferView);
                primitive.SetAttribute("TEXCOORD_0", uvAccessor);

                
                //SKELETON
                Accessor jointsAccessor = null;
                if (skin.State.HasFlag(SkinState.SkeletonLoaded))
                {
                    jointsAccessor = gltfAsset.CreateAccessor(ComponentType.UShort, AccessorType.Vec4)
                        .SetBufferView(attributesBufferView);
                    primitive.SetAttribute("JOINTS_0", jointsAccessor);
                }


                var weightsAccessor = gltfAsset.CreateAccessor(ComponentType.Float, AccessorType.Vec4)
                    .SetBufferView(attributesBufferView);
                primitive.SetAttribute("WEIGHTS_0", weightsAccessor);
                var writeColour = subMesh.Vertices.Select(vertex => vertex.Color).All(color => color != null);
                var colourAccessor = writeColour
                    ? gltfAsset.CreateAccessor(ComponentType.Float, AccessorType.Vec4)
                        .SetBufferView(attributesBufferView)
                    : null;
                if (writeColour)
                    primitive.SetAttribute("COLOR_0", colourAccessor);
                foreach (var vertex in subMesh.Vertices)
                {
                    positionAccessor.WriteElement(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
                    var normalLength = vertex.Normal.Length();
                    if (normalLength is 0 or float.NaN)
                    {
                        vertex.Normal = vertex.Position;
                        normalLength = vertex.Normal.Length();
                    }

                    if (normalLength is not 1f)
                        vertex.Normal = Vector3.Normalize(vertex.Normal);
                    normalAccessor.WriteElement(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z);
                    var uvX = vertex.UV.X;
                    var uvY = vertex.UV.Y;
                    if (float.IsPositiveInfinity(vertex.UV.X))
                        uvX = float.MaxValue;
                    else if (float.IsNegativeInfinity(vertex.UV.X))
                        uvX = float.MinValue;
                    if (float.IsPositiveInfinity(vertex.UV.Y))
                        uvY = float.MaxValue;
                    else if (float.IsNegativeInfinity(vertex.UV.Y))
                        uvY = float.MinValue;
                    uvAccessor.WriteElement(uvX, uvY);
                    colourAccessor?.WriteElement(vertex.Color!.Value.R, vertex.Color.Value.G,
                        vertex.Color.Value.B,
                        vertex.Color.Value.A);
                    weightsAccessor.WriteElement(vertex.Weights[0], vertex.Weights[1], vertex.Weights[2],
                        vertex.Weights[3]);
                    if (!skin.State.HasFlag(SkinState.SkeletonLoaded))
                        continue;
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
            scene.AddNode(skeletonRootNode);
            var inverseBindMatricesBufferView = gltfAsset.CreateBufferView(buffer);
            var inverseBindMatricesAccessor = gltfAsset.CreateAccessor(ComponentType.Float, AccessorType.Mat4);
            inverseBindMatricesAccessor.SetBufferView(inverseBindMatricesBufferView);
            var joints = new Dictionary<SkeletonJoint, Node>();
            foreach (var skeletonJoint in skin.Skeleton.Joints)
            {
                inverseBindMatricesAccessor.WriteElement(skeletonJoint.InverseBindTransform
                    .FixInverseBindMatrix()
                    .Rotate()
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

            return gltfAsset;
        }
    }
}