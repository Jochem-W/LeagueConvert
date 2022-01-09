using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImageMagick;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json.Extensions;

public static class GltfAssetExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static UShortAccessor CreateUShortAccessor(this GltfAsset gltfAsset, BufferView bufferView,
        AccessorType accessorType, bool minMax = false)
    {
        return new UShortAccessor(gltfAsset, bufferView, accessorType, minMax);
    }

    public static FloatAccessor CreateFloatAccessor(this GltfAsset gltfAsset, BufferView bufferView,
        AccessorType accessorType, bool minMax = false)
    {
        return new FloatAccessor(gltfAsset, bufferView, accessorType, minMax);
    }

    public static Animation CreateAnimation(this GltfAsset gltfAsset, string name = null)
    {
        return new Animation(gltfAsset) {Name = name};
    }

    public static Asset CreateAsset(this GltfAsset gltfAsset, string copyright = null)
    {
        return new Asset(gltfAsset) {Copyright = copyright};
    }

    public static Buffer CreateBuffer(this GltfAsset gltfAsset)
    {
        return new Buffer(gltfAsset);
    }

    public static BufferView CreateBufferView(this GltfAsset gltfAsset, Buffer buffer,
        BufferViewTarget? target = null)
    {
        return new BufferView(gltfAsset, buffer) {Target = target};
    }

    public static async Task<Image> CreateImage(this GltfAsset gltfAsset, BufferView bufferView,
        IMagickImage magickImage, string name = null)
    {
        if (bufferView.Accessors.Count > 0)
            throw new ArgumentException("BufferView can't be referenced by accessors", nameof(bufferView));
        if (bufferView.BinaryWriter.BaseStream.Length != 0)
            throw new ArgumentException("BufferView has to be empty", nameof(bufferView));
        await magickImage.WriteAsync(bufferView.BinaryWriter.BaseStream, MagickFormat.Png);
        return new Image(gltfAsset, bufferView, MimeType.Png) {Name = name};
    }

    public static Material CreateMaterial(this GltfAsset gltfAsset, string name = null)
    {
        return new Material(gltfAsset) {Name = name};
    }

    public static Mesh CreateMesh(this GltfAsset gltfAsset)
    {
        return new Mesh(gltfAsset);
    }

    public static Node CreateNode(this GltfAsset gltfAsset, string name = null)
    {
        return CreateNode(gltfAsset, Matrix4x4.Identity, name);
    }

    public static Node CreateNode(this GltfAsset gltfAsset, Matrix4x4 transform, string name = null)
    {
        return new Node(gltfAsset, transform, name);
    }

    public static Sampler CreateSampler(this GltfAsset gltfAsset, WrappingMode? wrapS = null,
        WrappingMode? wrapT = null)
    {
        return new Sampler(gltfAsset) {WrapS = wrapS, WrapT = wrapT};
    }

    public static Scene CreateScene(this GltfAsset gltfAsset)
    {
        return new Scene(gltfAsset);
    }

    public static Skin CreateSkin(this GltfAsset gltfAsset)
    {
        return new Skin(gltfAsset);
    }

    public static Texture CreateTexture(this GltfAsset gltfAsset, Sampler sampler = null, Image image = null)
    {
        return new Texture(gltfAsset) {Sampler = sampler, Source = image};
    }

    public static async Task Save(this GltfAsset gltfAsset, string path)
    {
        gltfAsset.Clean();
        await gltfAsset.WriteBuffers();
        await (Path.GetExtension(path) switch
        {
            ".glb" => gltfAsset.SaveGltfBinary(path),
            _ => gltfAsset.SaveGltfBinary(path, true)
        });
    }

    private static async Task SaveGltfBinary(this GltfAsset gltfAsset, string filePath, bool external = false)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await using var binaryWriter = new BinaryWriter(new MemoryStream());
        binaryWriter.Write("glTF".ToMagic());
        binaryWriter.Write((uint) 2);
        binaryWriter.Seek(4, SeekOrigin.Current);

        await WriteJson(binaryWriter, gltfAsset);

        foreach (var buffer in gltfAsset.Buffers)
            await WriteChunk(binaryWriter, "BIN", buffer.Stream);

        binaryWriter.Seek(8, SeekOrigin.Begin);
        binaryWriter.Write((uint) binaryWriter.BaseStream.Length);
        binaryWriter.Seek(0, SeekOrigin.Begin);
        await using var fileStream = File.Create(filePath);
        await binaryWriter.BaseStream.CopyToAsync(fileStream);
    }

    private static async Task WriteJson(BinaryWriter binaryWriter, GltfAsset gltfAsset)
    {
        await using var jsonStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(jsonStream, gltfAsset, JsonSerializerOptions);
        await WriteChunk(binaryWriter, "JSON", jsonStream, ' ');
    }

    private static async Task WriteChunk(BinaryWriter binaryWriter, string magic, Stream data, char? padding = null)
    {
        var headerStart = binaryWriter.BaseStream.Position;
        binaryWriter.Seek(4, SeekOrigin.Current);
        binaryWriter.Write(magic.ToMagic());
        var start = binaryWriter.BaseStream.Position;
        data.Seek(0, SeekOrigin.Begin);
        await data.CopyToAsync(binaryWriter.BaseStream);
        var offset = binaryWriter.BaseStream.Position.GetOffset(4);
        if (padding.HasValue)
            for (var i = 0; i < offset; i++)
                binaryWriter.Write(padding.Value);
        else
            binaryWriter.Write(new byte[offset]);
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

    private static async Task WriteBuffers(this GltfAsset gltfAsset)
    {
        foreach (var buffer in gltfAsset.BufferList)
        {
            if (buffer.Stream != null)
                await buffer.Stream.DisposeAsync();
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

    private static void Clean(this GltfAsset gltfAsset)
    {
        // TODO: maybe check accessors too
        if (gltfAsset.BufferViews != null)
            gltfAsset.CleanBufferViews();
        if (gltfAsset.Animations != null)
            gltfAsset.CleanAnimations();
        if (gltfAsset.Buffers != null)
            gltfAsset.CleanBuffers();
    }

    private static void CleanBuffers(this GltfAsset gltfAsset)
    {
        var removed = 0;
        for (var i = 0; i < gltfAsset.BufferList.Count; i++)
        {
            var buffer = gltfAsset.BufferList[i];
            if (buffer.BufferViews.Count == 0)
            {
                gltfAsset.BufferList.RemoveAt(i);
                removed++;
                i--;
                continue;
            }

            buffer.Index -= removed;
        }
    }
    
    private static void CleanAnimations(this GltfAsset gltfAsset)
    {
        for (var i = 0; i < gltfAsset.AnimationList.Count; i++)
        {
            var animation = gltfAsset.AnimationList[i];
            if (animation.ChannelList.Count != 0 && animation.SamplerList.Count != 0)
                continue;
            gltfAsset.AnimationList.Remove(animation);
            i--;
        }
    }

    private static void CleanBufferViews(this GltfAsset gltfAsset)
    {
        var removed = 0;
        for (var i = 0; i < gltfAsset.BufferViewList.Count; i++)
        {
            var bufferView = gltfAsset.BufferViewList[i];
            if (bufferView.ByteLength == 0 && bufferView.Accessors.Count == 0)
            {
                gltfAsset.BufferViewList.Remove(bufferView);
                bufferView.Buffer.BufferViews.Remove(bufferView);
                removed++;
                i--;
                continue;
            }

            bufferView.Index -= removed;
        }
    }
}