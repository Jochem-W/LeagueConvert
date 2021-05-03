using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SimpleGltf.Extensions;
using SimpleGltf.Json;
using Buffer = SimpleGltf.Json.Buffer;

namespace SimpleGltf.IO
{
    public class SimpleGltfAsset
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            IgnoreNullValues = true
        };

        internal readonly Buffer Buffer;

        internal readonly GltfAsset GltfAsset;
        private IList<SimpleScene> _scenes;

        public SimpleGltfAsset()
        {
            GltfAsset = new GltfAsset();
            Buffer = new Buffer(GltfAsset);
        }

        public string Copyright
        {
            get => GltfAsset.Asset.Copyright;
            set => GltfAsset.Asset.Copyright = value;
        }

        public IEnumerable<SimpleScene> Scenes => _scenes;

        private async Task WriteBuffer(string folder)
        {
            await using var fileStream = File.Create(Path.Combine(folder, Buffer.Uri));
            await using var stream = await Buffer.GetStreamAsync();
            await stream.CopyToAsync(fileStream);
        }

        public SimpleScene CreateScene()
        {
            _scenes ??= new List<SimpleScene>();
            var scene = new SimpleScene(this);
            _scenes.Add(scene);
            return scene;
        }

        public Task Save(string path)
        {
            return Path.GetExtension(path) switch
            {
                ".glb" => SaveGltfBinary(path),
                ".gltf" => SaveGltfEmbedded(path),
                _ => SaveGltf(path)
            };
        }

        public async Task SaveGltfBinary(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await using var binaryWriter = new BinaryWriter(File.Create(filePath));
            binaryWriter.Write("glTF".ToMagic());
            binaryWriter.Write((uint) 2);
            binaryWriter.Seek(4, SeekOrigin.Current);
            var headerEnd = binaryWriter.BaseStream.Position;

            binaryWriter.Seek(4, SeekOrigin.Current);
            binaryWriter.Write("JSON".ToMagic());
            await JsonSerializer.SerializeAsync(binaryWriter.BaseStream, GltfAsset, JsonSerializerOptions);
            var offset = binaryWriter.BaseStream.Position.GetOffset();
            for (var i = 0; i < offset; i++)
                binaryWriter.Write(' ');
            var jsonEnd = binaryWriter.BaseStream.Position;
            var jsonLength = binaryWriter.BaseStream.Position - headerEnd - 8;

            binaryWriter.Seek(4, SeekOrigin.Current);
            binaryWriter.Write("BIN".ToMagic());
            await using var stream = await Buffer.GetStreamAsync();
            await stream.CopyToAsync(binaryWriter.BaseStream);
            offset = binaryWriter.BaseStream.Position.GetOffset();
            binaryWriter.Write(new byte[offset]);
            var binaryLength = binaryWriter.BaseStream.Position - jsonEnd - 8;

            binaryWriter.Seek(8, SeekOrigin.Begin);
            binaryWriter.Write((uint) binaryWriter.BaseStream.Length);
            binaryWriter.Seek((int) headerEnd, SeekOrigin.Begin);
            binaryWriter.Write((uint) jsonLength);
            binaryWriter.Seek((int) jsonEnd, SeekOrigin.Begin);
            binaryWriter.Write((uint) binaryLength);
        }

        public async Task SaveGltfEmbedded(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            await using var stream = await Buffer.GetStreamAsync();
            var bytes = new byte[stream.Length];

            await stream.ReadAsync(bytes);
            Buffer.Uri = $"data:application/octet-stream;base64,{Convert.ToBase64String(bytes)}";
            await using var fileStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fileStream, GltfAsset, JsonSerializerOptions);
        }

        public async Task SaveGltf(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            Buffer.Uri = "data.bin";
            await WriteBuffer(directoryPath);
            await using var fileStream = File.Create(Path.Combine(directoryPath, "model.gltf"));
            await JsonSerializer.SerializeAsync(fileStream, GltfAsset, JsonSerializerOptions);
        }
    }
}