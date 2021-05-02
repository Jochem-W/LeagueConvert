using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using LeagueConvert.IO.HashTables;
using LeagueConvert.IO.WadFile;
using LeagueToolkit.IO.SimpleSkinFile;
using SimpleGltf.IO;
using SimpleGltf.IO.Accessors;

namespace SimpleGltfTest
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            await HashTables.TryLoadLatest();
            foreach (var file in Directory.EnumerateFiles(@"C:\Riot Games\League of Legends", "*.wad.client",
                SearchOption.AllDirectories))
            {
                using var wad = new StringWad(file);
                //using var wad = new StringWad(@"C:\Riot Games\League of Legends\Game\DATA\FINAL\Champions\Warwick.wad.client");
                foreach (var (name, entry) in wad.Entries.Where(pair => Path.GetExtension(pair.Key) == ".skn"))
                {
                    var simpleSkin = new SimpleSkin(entry.GetStream());
                    await using var gltf = new SimpleGltfAsset();
                    var root = gltf.CreateScene().CreateNode("root");
                    foreach (var subMesh in simpleSkin.Submeshes)
                    {
                        var primitive = root.CreateChild(subMesh.Name).CreateMesh().CreatePrimitive();
                        var positionAccessor = primitive.CreatePositionAccessor();
                        var normalAccessor = primitive.CreateNormalAccessor();
                        var uvAccessor = primitive.CreateUvAccessor<float>();
                        var jointsAccessor = primitive.CreateJointsAccessor<byte>();
                        var weightsAccessor = primitive.CreateWeightsAccessor<float>();
                        SimpleVector4Accessor<float> colorAccessor = null;
                        var writeColor = subMesh.Vertices.Select(vertex => vertex.Color).All(color => color != null);
                        if (writeColor)
                            colorAccessor = primitive.CreateRgbaColorAccessor<float>();
                        foreach (var vertex in subMesh.Vertices)
                        {
                            positionAccessor.Write(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
                            var normalLength = vertex.Normal.Length();
                            if (normalLength is 0 or float.NaN)
                            {
                                vertex.Normal = vertex.Position;
                                normalLength = vertex.Normal.Length();
                            }

                            if (Math.Abs(normalLength - 1) > float.Epsilon)
                                vertex.Normal = Vector3.Normalize(vertex.Normal);
                            normalAccessor.Write(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z);
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
                            vertex.UV = new Vector2(uvX, uvY);
                            uvAccessor.Write(vertex.UV.X, vertex.UV.Y);
                            if (writeColor)
                                colorAccessor.Write(vertex.Color!.Value.R, vertex.Color.Value.G, vertex.Color.Value.B,
                                    vertex.Color.Value.A);
                            jointsAccessor.Write(vertex.BoneIndices[0], vertex.BoneIndices[1], vertex.BoneIndices[2],
                                vertex.BoneIndices[3]);
                            weightsAccessor.Write(vertex.Weights[0], vertex.Weights[1], vertex.Weights[2],
                                vertex.Weights[3]);
                        }

                        var indicesAccessor = primitive.CreateIndicesAccessor<ushort>();
                        foreach (var index in subMesh.Indices)
                            indicesAccessor.Write(index);
                    }

                    // gltf
                    await gltf.Save(Path.Combine(@"C:\Users\Joche\Downloads", "models", Path.GetDirectoryName(name),
                        Path.GetFileNameWithoutExtension(name)));

                    // gltf embedded
                    /*await gltf.Save(Path.Combine(@"C:\Users\Joche\Downloads", "models", Path.GetDirectoryName(name),
                        Path.ChangeExtension(Path.GetFileName(name), ".gltf")));*/

                    // gltf binary
                    await gltf.Save(Path.Combine(@"C:\Users\Joche\Downloads", "models", Path.GetDirectoryName(name),
                        Path.ChangeExtension(Path.GetFileName(name), ".glb")));
                }
            }
        }
    }
}