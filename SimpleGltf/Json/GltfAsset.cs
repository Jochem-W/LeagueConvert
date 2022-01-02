using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json;

public class GltfAsset : IAsyncDisposable
{
    private static readonly IList<string> ExtensionsUsedList = new List<string> {"KHR_materials_unlit"};
    internal readonly IList<Accessor> AccessorList = new List<Accessor>();
    internal readonly IList<Animation> AnimationList = new List<Animation>();
    internal readonly IList<Buffer> BufferList = new List<Buffer>();
    internal readonly IList<BufferView> BufferViewList = new List<BufferView>();
    internal readonly IList<Image> ImageList = new List<Image>();
    internal readonly IList<Material> MaterialList = new List<Material>();
    internal readonly IList<Mesh> MeshList = new List<Mesh>();
    internal readonly IList<Node> NodeList = new List<Node>();
    internal readonly IList<Sampler> SamplerList = new List<Sampler>();
    internal readonly IList<Scene> SceneList = new List<Scene>();
    internal readonly IList<Skin> SkinList = new List<Skin>();
    internal readonly IList<Texture> TextureList = new List<Texture>();

    public GltfAsset()
    {
        this.CreateAsset();
    }

    public IEnumerable<Accessor> Accessors => AccessorList.Count > 0 ? AccessorList : null;

    public IEnumerable<Animation> Animations => AnimationList.Count > 0 ? AnimationList : null;

    public Asset Asset { get; internal set; }

    public IEnumerable<Buffer> Buffers => BufferList.Count > 0 ? BufferList : null;

    public IEnumerable<BufferView> BufferViews => BufferViewList.Count > 0 ? BufferViewList : null;

    public IEnumerable<string> ExtensionsUsed => MaterialList.Count > 0 ? ExtensionsUsedList : null;

    public IEnumerable<Image> Images => ImageList.Count > 0 ? ImageList : null;

    public IEnumerable<Material> Materials => MaterialList.Count > 0 ? MaterialList : null;

    public IEnumerable<Mesh> Meshes => MeshList.Count > 0 ? MeshList : null;

    public IEnumerable<Node> Nodes => NodeList.Count > 0 ? NodeList : null;

    public IEnumerable<Sampler> Samplers => SamplerList.Count > 0 ? SamplerList : null;

    [JsonConverter(typeof(IndexableConverter<Scene>))]
    public Scene Scene { get; set; }

    public IEnumerable<Scene> Scenes => SceneList.Count > 0 ? SceneList : null;

    public IEnumerable<Skin> Skins => SkinList.Count > 0 ? SkinList : null;

    public IEnumerable<Texture> Textures => TextureList.Count > 0 ? TextureList : null;

    public async ValueTask DisposeAsync()
    {
        if (Buffers == null)
            return;
        foreach (var buffer in BufferList)
            await buffer.Stream.DisposeAsync();
        foreach (var bufferView in BufferViewList)
            await bufferView.BinaryWriter.DisposeAsync();
    }
}