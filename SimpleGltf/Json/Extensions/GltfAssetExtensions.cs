using System;
using System.IO;
using System.Numerics;
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

        public static Buffer CreateBuffer(this GltfAsset gltfAsset)
        {
            return new(gltfAsset);
        }

        public static Image CreateImage(this GltfAsset gltfAsset, string uri, string name = null)
        {
            return new(gltfAsset, uri, name);
        }

        public static async Task<Image> CreateImage(this GltfAsset gltfAsset, BufferView bufferView,
            IMagickImage magickImage, string name = null)
        {
            if (bufferView.Accessors.Count > 0)
                throw new ArgumentException("BufferView can't be referenced by accessors", nameof(bufferView));
            if (bufferView.BinaryWriter.BaseStream.Length != 0)
                throw new ArgumentException("BufferView has to be empty", nameof(bufferView));
            await magickImage.WriteAsync(bufferView.BinaryWriter.BaseStream, MagickFormat.Png);
            return new Image(gltfAsset, bufferView, MimeType.Png, name);
        }

        public static Material CreateMaterial(this GltfAsset gltfAsset, string name = null)
        {
            return new(gltfAsset) {Name = name};
        }

        public static Mesh CreateMesh(this GltfAsset gltfAsset)
        {
            return new(gltfAsset);
        }

        public static Node CreateNode(this GltfAsset gltfAsset, string name = null)
        {
            return CreateNode(gltfAsset, Matrix4x4.Identity, name);
        }

        public static Node CreateNode(this GltfAsset gltfAsset, Matrix4x4 transform, string name = null)
        {
            return new(gltfAsset, transform, name);
        }

        public static Sampler CreateSampler(this GltfAsset gltfAsset, WrappingMode? wrapS = null,
            WrappingMode? wrapT = null)
        {
            return new(gltfAsset) {WrapS = wrapS, WrapT = wrapT};
        }

        public static Scene CreateScene(this GltfAsset gltfAsset)
        {
            return new(gltfAsset);
        }

        public static Skin CreateSkin(this GltfAsset gltfAsset)
        {
            return new(gltfAsset);
        }

        public static Texture CreateTexture(this GltfAsset gltfAsset, Sampler sampler = null, Image image = null)
        {
            return new(gltfAsset) {Sampler = sampler, Source = image};
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
                await WriteChunk(binaryWriter, "BIN", buffer.Stream);

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
            binaryWriter.Write(new byte[binaryWriter.BaseStream.Position.GetOffset(4)]);
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
            var offset = binaryWriter.BaseStream.Position.GetOffset(4);
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
            foreach (var buffer in gltfAsset.Buffers)
            {
                buffer.Stream = new MemoryStream();
                foreach (var bufferView in buffer.BufferViews)
                {
                    var firstAccessor = bufferView.Accessors.Count > 0 ? bufferView.Accessors[0] : null;
                    if (bufferView.Target == BufferViewTarget.ArrayBuffer ||
                        firstAccessor?.Type is AccessorType.Mat2 or AccessorType.Mat3 or AccessorType.Mat4)
                        buffer.Stream.Seek(buffer.Stream.Position.GetOffset(4), SeekOrigin.Current);
                    if (firstAccessor != null)
                        buffer.Stream.Seek(buffer.Stream.Position.GetOffset(firstAccessor.ComponentType.GetSize()),
                            SeekOrigin.Current);
                    bufferView.ActualByteOffset = (int) buffer.Stream.Position;
                    bufferView.BinaryWriter.Seek(0, SeekOrigin.Begin);
                    await bufferView.BinaryWriter.BaseStream.CopyToAsync(buffer.Stream);
                }
            }
        }
    }
}