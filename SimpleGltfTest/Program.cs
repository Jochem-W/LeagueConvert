using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using LeagueConvert.IO.HashTables;
using LeagueConvert.IO.WadFile;
using LeagueToolkit.IO.SimpleSkinFile;
using SimpleGltf.Enums;
using SimpleGltf.Json;
using SimpleGltf.Json.Extensions;

namespace SimpleGltfTest
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            await HashTables.TryLoadLatest();
            if (Directory.Exists(@"C:\Users\Joche\Downloads\models"))
                Directory.Delete(@"C:\Users\Joche\Downloads\models", true);
            using var wad = new StringWad(@"C:\Riot Games\League of Legends\Game\DATA\FINAL\Champions\Warwick.wad.client");
            await Test(wad);
            /*foreach (var file in Directory.EnumerateFiles(@"C:\Riot Games\League of Legends", "*.wad.client",
                SearchOption.AllDirectories))
            {
                using var wad = new StringWad(file);
                await Test(wad);
            }*/
        }

        private static async Task Test(StringWad wad)
        {
            foreach (var (name, entry) in wad.Entries.Where(pair => Path.GetExtension(pair.Key) == ".skn"))
            {
                var simpleSkin = new SimpleSkin(entry.GetStream());
                var gltfAsset = new GltfAsset();
                var buffer = gltfAsset.CreateBuffer();
                var scene = gltfAsset.CreateScene();
                var node = gltfAsset.CreateNode("root");
                scene.AddNode(node);
                node.Mesh = gltfAsset.CreateMesh();
                //node.Scale = new Vector3(1, 1, -1);
                foreach (var subMesh in simpleSkin.Submeshes)
                {
                    var primitive = node.Mesh.CreatePrimitive();
                    var indicesBufferView = gltfAsset.CreateBufferView(buffer, BufferViewTarget.ElementArrayBuffer);
                    var indicesAccessor = gltfAsset.CreateAccessor(ComponentType.UShort, AccessorType.Scalar)
                        .SetBufferView(indicesBufferView);
                    primitive.Indices = indicesAccessor;
                    foreach (var index in subMesh.Indices)
                        indicesAccessor.WriteComponent(index);
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
                    var jointsAccessor = gltfAsset.CreateAccessor(ComponentType.Byte, AccessorType.Vec4)
                        .SetBufferView(attributesBufferView);
                    primitive.SetAttribute("JOINTS_0", jointsAccessor);
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
                        positionAccessor.WriteComponent(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
                        var normalLength = vertex.Normal.Length();
                        if (normalLength is 0 or float.NaN)
                        {
                            vertex.Normal = vertex.Position;
                            normalLength = vertex.Normal.Length();
                        }

                        if (normalLength is not 1f)
                            vertex.Normal = Vector3.Normalize(vertex.Normal);
                        normalAccessor.WriteComponent(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z);
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
                        uvAccessor.WriteComponent(uvX, uvY);
                        colourAccessor?.WriteComponent(vertex.Color!.Value.R, vertex.Color.Value.G,
                            vertex.Color.Value.B,
                            vertex.Color.Value.A);
                        jointsAccessor.WriteComponent(vertex.BoneIndices[0], vertex.BoneIndices[1],
                            vertex.BoneIndices[2],
                            vertex.BoneIndices[3]);
                        weightsAccessor.WriteComponent(vertex.Weights[0], vertex.Weights[1], vertex.Weights[2],
                            vertex.Weights[3]);
                    }

                    var material = gltfAsset.CreateMaterial(name: subMesh.Name);
                    primitive.Material = material;
                    
                }

                await gltfAsset.Save(Path.Combine(@"C:\Users\Joche\Downloads", "models", Path.GetDirectoryName(name),
                    Path.ChangeExtension(Path.GetFileName(name), ".glb")));
            }
        }
    }
}