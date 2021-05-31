using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ImageMagick;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json.Extensions
{
    public static class GltfAssetExtensions
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            IgnoreNullValues = true
        };

        public static Animation CreateAnimation(this GltfAsset gltfAsset, string name = null)
        {
            return new(gltfAsset, name);
        }

        public static Asset CreateAsset(this GltfAsset gltfAsset, string copyright = null)
        {
            return new(gltfAsset) {Copyright = copyright};
        }

        public static Buffer CreateBuffer(this GltfAsset gltfAsset, string name = null)
        {
            return new(gltfAsset, name);
        }

        public static Image CreateImage(this GltfAsset gltfAsset, string uri, string name = null)
        {
            return new(gltfAsset, uri, name);
        }

        public static async Task<Image> CreateImage(this GltfAsset gltfAsset, BufferView bufferView,
            IMagickImage magickImage, string name = null)
        {
            if (bufferView.PngStream != null || bufferView.GetAccessors().Any())
                throw new NotImplementedException();
            bufferView.PngStream = new MemoryStream();
            await magickImage.WriteAsync(bufferView.PngStream, MagickFormat.Png);
            bufferView.PngStream.Seek(0, SeekOrigin.Begin);
            return new Image(gltfAsset, bufferView, MimeType.Png, name);
        }

        public static Material CreateMaterial(this GltfAsset gltfAsset, Vector3? emissiveFactor = null,
            AlphaMode? alphaMode = null, float? alphaCutoff = null, bool? doubleSided = null, string name = null)
        {
            return new(gltfAsset, emissiveFactor, alphaMode, alphaCutoff, doubleSided, name);
        }

        public static Mesh CreateMesh(this GltfAsset gltfAsset, string name = null)
        {
            return new(gltfAsset, name);
        }

        public static Node CreateNode(this GltfAsset gltfAsset, string name = null)
        {
            return CreateNode(gltfAsset, Matrix4x4.Identity, name);
        }

        public static Node CreateNode(this GltfAsset gltfAsset, Matrix4x4 transform, string name = null)
        {
            return new(gltfAsset, transform, name);
        }

        public static Node CreateNode(this GltfAsset gltfAsset, Quaternion rotation, Vector3 scale,
            Vector3 translation, string name = null)
        {
            return new(gltfAsset, rotation, scale, translation, name);
        }

        public static Sampler CreateSampler(this GltfAsset gltfAsset, ScaleFilter? magFilter = null,
            ScaleFilter? minFilter = null, WrappingMode wrapS = WrappingMode.Repeat,
            WrappingMode wrapT = WrappingMode.Repeat, string name = null)
        {
            return new(gltfAsset, magFilter, minFilter, wrapS, wrapT, name);
        }

        public static Scene CreateScene(this GltfAsset gltfAsset, string name = null)
        {
            var scene = new Scene(gltfAsset, name);
            return scene;
        }

        public static Skin CreateSkin(this GltfAsset gltfAsset, string name = null)
        {
            return new(gltfAsset, name);
        }

        public static Texture CreateTexture(this GltfAsset gltfAsset, Sampler sampler, Image image,
            string name = null)
        {
            return new(gltfAsset, sampler, image, name);
        }

        public static async Task Save(this GltfAsset gltfAsset, string path)
        {
            await gltfAsset.WriteBuffers();
            await (Path.GetExtension(path) switch
            {
                ".glb" => gltfAsset.SaveGltfBinary(path),
                ".gltf" => gltfAsset.SaveGltfEmbedded(path),
                _ => gltfAsset.SaveGltf(path)
            });
        }

        public static async Task SaveGltfBinary(this GltfAsset gltfAsset, string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await using var binaryWriter = new BinaryWriter(new MemoryStream());
            binaryWriter.Write("glTF".ToMagic());
            binaryWriter.Write((uint) 2);
            binaryWriter.Seek(4, SeekOrigin.Current);

            await WriteChunk(binaryWriter, gltfAsset);

            foreach (var buffer in gltfAsset.Buffers)
            {
                await WriteChunk(binaryWriter, "BIN", buffer.MemoryStream);
            }

            binaryWriter.Seek(8, SeekOrigin.Begin);
            binaryWriter.Write((uint) binaryWriter.BaseStream.Length);
            binaryWriter.Seek(0, SeekOrigin.Begin);
            await using var fileStream = File.Create(filePath);
            await binaryWriter.BaseStream.CopyToAsync(fileStream);
        }

        //TODO: merge the following two functions
        private static async Task WriteChunk(BinaryWriter binaryWriter, string magic, Stream data)
        {
            var headerStart = binaryWriter.BaseStream.Position;
            binaryWriter.Seek(4, SeekOrigin.Current);
            binaryWriter.Write(magic.ToMagic());
            var start = binaryWriter.BaseStream.Position;
            data.Seek(0, SeekOrigin.Begin);
            await data.CopyToAsync(binaryWriter.BaseStream);
            binaryWriter.Write(new byte[binaryWriter.BaseStream.Position.GetOffset()]);
            var end = binaryWriter.BaseStream.Position;
            var length = end - start;
            binaryWriter.Seek((int) headerStart, SeekOrigin.Begin);
            binaryWriter.Write((uint) length);
            binaryWriter.Seek((int) end, SeekOrigin.Begin);
        }

        private static async Task WriteChunk(BinaryWriter binaryWriter, GltfAsset gltfAsset)
        {
            var headerStart = binaryWriter.BaseStream.Position;
            binaryWriter.Seek(4, SeekOrigin.Current);
            binaryWriter.Write("JSON".ToMagic());
            var start = binaryWriter.BaseStream.Position;
            await JsonSerializer.SerializeAsync(binaryWriter.BaseStream, gltfAsset, JsonSerializerOptions);
            var offset = binaryWriter.BaseStream.Position.GetOffset();
            for (var i = 0; i < offset; i++)
                binaryWriter.Write(' ');
            var end = binaryWriter.BaseStream.Position;
            var length = end - start;
            binaryWriter.Seek((int) headerStart, SeekOrigin.Begin);
            binaryWriter.Write((uint) length);
            binaryWriter.Seek((int) end, SeekOrigin.Begin);
        }

        public static Task SaveGltfEmbedded(this GltfAsset gltfAsset, string filePath)
        {
            throw new NotImplementedException();
        }

        public static Task SaveGltf(this GltfAsset gltfAsset, string folderPath)
        {
            throw new NotImplementedException();
        }

        private static async Task WriteBuffers(this GltfAsset gltfAsset, bool external = false)
        {
            for (var i = 0; i < gltfAsset.Buffers.Count; i++)
            {
                var buffer = gltfAsset.Buffers[i];
                buffer.Uri = external ? $"buffer_{i}.bin" : null;
                buffer.MemoryStream = new MemoryStream();
                var binaryWriter = new BinaryWriter(buffer.MemoryStream, Encoding.Default, true);
                foreach (var bufferView in gltfAsset.BufferViews.Where(bufferView => bufferView.Buffer == buffer))
                {
                    if (bufferView.Target == BufferViewTarget.ArrayBuffer)
                        binaryWriter.Seek((int) binaryWriter.BaseStream.Position.GetOffset(), SeekOrigin.Current);
                    bufferView.ByteOffset = (int) binaryWriter.BaseStream.Position;
                    var accessors = bufferView.GetAccessors().ToList();
                    switch (accessors.Count)
                    {
                        case 1:
                        {
                            var accessor = accessors[0];
                            for (var j = 0; j < accessor.Count; j++)
                                accessor.WriteToBinaryWriter(binaryWriter, j);
                            break;
                        }
                        case > 1:
                        {
                            var counts = accessors.Select(accessor => accessor.Count).Distinct().ToList();
                            if (counts.Count != 1)
                                throw new NotImplementedException();
                            var stride = accessors.GetStride(bufferView.Target == BufferViewTarget.ArrayBuffer);
                            bufferView.ByteStride = stride.Total;
                            for (var j = 0; j < accessors.Count; j++)
                                accessors[j].ByteOffset = stride.Offsets[j];
                            var count = counts[0];
                            for (var j = 0; j < count; j++)
                            for (var k = 0; k < accessors.Count; k++)
                            {
                                var nextPosition = buffer.MemoryStream.Position + stride.Lengths[k];
                                accessors[k].WriteToBinaryWriter(binaryWriter, j);
                                binaryWriter.Seek((int) nextPosition, SeekOrigin.Begin);
                            }

                            break;
                        }
                        case 0:
                        {
                            if (bufferView.PngStream == null)
                                throw new NotImplementedException();
                            await bufferView.PngStream.CopyToAsync(buffer.MemoryStream);
                            break;
                        }
                    }

                    bufferView.ByteLength = (int) (binaryWriter.BaseStream.Position - bufferView.ByteOffset);
                }

                await binaryWriter.DisposeAsync();
            }
        }
    }
}