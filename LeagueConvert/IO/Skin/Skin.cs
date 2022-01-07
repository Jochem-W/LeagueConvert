using ImageMagick;
using LeagueConvert.Enums;
using LeagueConvert.Helpers;
using LeagueConvert.IO.PropertyBin;
using LeagueConvert.IO.WadFile;
using LeagueToolkit.IO.AnimationFile;
using LeagueToolkit.IO.PropertyBin;
using LeagueToolkit.IO.PropertyBin.Properties;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.IO.SkeletonFile;
using Serilog;

namespace LeagueConvert.IO.Skin;

public class Skin : IDisposable
{
    private readonly IDictionary<string, string> _animationFiles;
    private readonly IList<Material> _materials;
    private readonly StringWad _parent;
    private IList<string> _hiddenSubMeshes;
    private uint? _material;
    private string _simpleSkinFile;
    private string _skeletonFile;
    private string _texture;
    public IDictionary<string, Animation> Animations;

    internal SimpleSkin SimpleSkin;
    internal Skeleton Skeleton;
    internal IDictionary<string, IMagickImage> Textures;

    internal Skin(string character, string name, ILogger logger = null, params ParentedBinTree[] binTrees)
    {
        logger?.Debug("Parsing {Character} skin{Id}", Character, Id);
        _animationFiles = new Dictionary<string, string>();
        _materials = new List<Material>();
        _parent = binTrees[0].Parent;
        Character = character;
        Id = int.Parse(name[4..]);
        foreach (var binTree in binTrees)
            ParseBinTree(binTree);
    }

    public string Character { get; }

    public int Id { get; }

    public string Name { get; private set; }

    public SkinState State { get; private set; }

    public void Dispose()
    {
        if (Textures != null)
            foreach (var image in Textures.Values)
                image.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task Load(SkinMode mode, ILogger logger = null)
    {
        logger?.Debug("Loading {Character} skin{Id}", Character, Id);
        if (!State.HasFlag(SkinState.MeshLoaded))
            if (!await TryLoadMesh(logger))
                return;
        if (!State.HasFlag(SkinState.TexturesLoaded))
            await TryLoadTextures(logger);
        if (mode == SkinMode.MeshAndTextures)
            return;

        if (!State.HasFlag(SkinState.SkeletonLoaded))
            await TryLoadSkeleton(logger);
        if (mode == SkinMode.WithSkeleton || !State.HasFlag(SkinState.SkeletonLoaded) ||
            State.HasFlag(SkinState.AnimationsLoaded))
            return;
        await LoadAnimations(logger);
    }

    private async Task<bool> TryLoadMesh(ILogger logger = null)
    {
        try
        {
            // TODO: check if _simpleSkinFile is null
            await using var stream = _parent.GetEntryByName(_simpleSkinFile).GetStream();
            SimpleSkin = new SimpleSkin(stream);
            State |= SkinState.MeshLoaded;
            return true;
        }
        catch (Exception e)
        {
            logger?.Warning(e, "Couldn't load mesh '{File}'", _simpleSkinFile);
            State = 0;
            return false;
        }
    }

    private async Task<bool> TryLoadTextures(ILogger logger = null)
    {
        _materials.Clean(_parent);
        Dictionary<string, Stream> streams = null;
        try
        {
            streams = new Dictionary<string, Stream>();
            foreach (var texture in _materials.GetTextures())
                streams[texture] = _parent.GetEntryByName(texture).GetStream();
            if (_texture != null && !streams.ContainsKey(_texture))
                streams[_texture] = _parent.GetEntryByName(_texture).GetStream();
            var images = new Dictionary<string, IMagickImage>();
            Textures = new Dictionary<string, IMagickImage>();
            foreach (var subMeshName in SimpleSkin.SubMeshes.Select(submesh => submesh.Name))
            {
                var texture = _texture;
                if (_materials.TryGetBySubMesh(subMeshName, out var material))
                    texture = material.Texture;
                if (texture == null)
                    continue;
                if (!images.ContainsKey(texture))
                    images[texture] = new MagickImage(streams[texture]);
                Textures[subMeshName] = images[texture];
            }

            State |= SkinState.TexturesLoaded;
            return true;
        }
        catch (Exception e)
        {
            logger?.Warning(e, "Unexpected error when loading textures");
            return false;
        }
        finally
        {
            if (streams != null)
                foreach (var stream in streams.Values)
                    await stream.DisposeAsync();
        }
    }

    private async Task<bool> TryLoadSkeleton(ILogger logger = null)
    {
        try
        {
            await using var stream = _parent.GetEntryByName(_skeletonFile).GetStream();
            Skeleton = new Skeleton(stream);
            State |= SkinState.SkeletonLoaded;
            return true;
        }
        catch (Exception e)
        {
            logger?.Warning(e, "Couldn't load skeleton '{File}'", _skeletonFile);
            return false;
        }
    }

    private async Task LoadAnimations(ILogger logger = null)
    {
        Animations = new Dictionary<string, Animation>();
        var streams = new Dictionary<string, Stream>();
        var animations = new Dictionary<string, Animation>();
        foreach (var (name, fileName) in _animationFiles)
            TryLoadAnimation(name, fileName, streams, animations, logger);
        foreach (var stream in streams.Values)
            await stream.DisposeAsync();
        State |= SkinState.AnimationsLoaded;
    }

    private bool TryLoadAnimation(string name, string fileName,
        IDictionary<string, Stream> streams, IDictionary<string, Animation> animations, ILogger logger = null)
    {
        try
        {
            if (animations.ContainsKey(fileName))
            {
                Animations.Add(name, animations[fileName]);
                return true;
            }

            if (!streams.ContainsKey(fileName))
                streams[fileName] = _parent.GetEntryByName(fileName).GetStream();
            animations.Add(fileName, new Animation(streams[fileName]));
            Animations.Add(name, animations[fileName]);
            return true;
        }
        catch (Exception e)
        {
            logger?.Warning(e, "Couldn't load animation '{File}'", fileName);
            return false;
        }
    }

    /*public void Save(string filePath, ILogger logger = null)
    {
        logger?.Debug("Saving {Character} skin{Id}", Character, Id);
        try
        {
            GetModelRoot().Save(filePath);
        }
        catch (Exception e)
        {
            logger?.Warning(e, "Unexpected error when saving");
        }
    }*/

    private void ParseBinTree(BinTree binTree)
    {
        foreach (var binTreeObject in binTree.Objects)
            switch (binTreeObject.MetaClassHash)
            {
                case 2607278582:
                    ParseSkinCharacterDataProperties(binTreeObject);
                    break;
                case 4126869447:
                    ParseAnimationGraphData(binTreeObject);
                    break;
                case 4288492553:
                    ParseStaticMaterialDef(binTreeObject);
                    break;
            }
    }

    private void ParseSkinCharacterDataProperties(BinTreeObject skinCharacterDataProperties)
    {
        foreach (var property in skinCharacterDataProperties.Properties)
            switch (property.NameHash)
            {
                case 762889000: // championSkinName
                    Name = ((BinTreeString) property).Value;
                    break;
                case 1174362372: // skinMeshProperties
                    ParseSkinMeshProperties((BinTreeEmbedded) property);
                    break;
            }
    }

    private void ParseSkinMeshProperties(BinTreeStructure skinMeshProperties)
    {
        switch (skinMeshProperties.MetaClassHash)
        {
            case 1628559524: // SkinMeshDataProperties
                foreach (var property in skinMeshProperties.Properties)
                    switch (property.NameHash)
                    {
                        case 2974586734: // skeleton
                            _skeletonFile = ((BinTreeString) property).Value;
                            break;
                        case 3600813558: // simpleSkin
                            _simpleSkinFile = ((BinTreeString) property).Value;
                            break;
                        case 1013213428: // texture
                            _texture = ((BinTreeString) property).Value;
                            break;
                        case 3538210912: // material
                            var material = ((BinTreeObjectLink) property).Value;
                            if (material != 0)
                                _material = material;
                            break;
                        case 2159540111: // initialSubmeshToHide
                            var initialSubmeshToHide = ((BinTreeString) property).Value;
                            if (initialSubmeshToHide != null)
                                _hiddenSubMeshes =
                                    initialSubmeshToHide.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            break;
                        case 611473680: // materialOverride
                            foreach (var embedded in ((BinTreeContainer) property).Properties.Cast<BinTreeEmbedded>())
                                AddMaterial(embedded);
                            break;
                    }

                break;
        }
    }

    private void AddMaterial(BinTreeStructure skinMeshDataPropertiesMaterialOverride)
    {
        uint? materialLink = null;
        string texture = null;
        string subMesh = null;
        foreach (var property in skinMeshDataPropertiesMaterialOverride.Properties)
            switch (property.NameHash)
            {
                case 3538210912: // materialLink
                    materialLink = ((BinTreeObjectLink) property).Value;
                    break;
                case 1013213428: // texture
                    texture = ((BinTreeString) property).Value;
                    break;
                case 2866241836: // submesh
                    subMesh = ((BinTreeString) property).Value;
                    break;
            }

        _materials.Add(new Material(materialLink, texture, subMesh));
    }

    private void ParseAnimationGraphData(BinTreeObject animationGraphData)
    {
        var mClipDataMap =
            (BinTreeMap) animationGraphData.Properties.FirstOrDefault(property => property.NameHash == 1172382456);
        if (mClipDataMap == null)
            return;
        ParseClipDataMap(mClipDataMap);
    }

    private void ParseClipDataMap(BinTreeMap mClipDataMap)
    {
        foreach (var (key, value) in mClipDataMap.Map.Select(pair =>
                     new KeyValuePair<BinTreeHash, BinTreeStructure>((BinTreeHash) pair.Key,
                         (BinTreeStructure) pair.Value)))
            switch (value.MetaClassHash)
            {
                case 1540989414: // atomicClipData
                    var hash = key.Value;
                    if (hash != 0)
                        AddAnimation(hash, value);
                    break;
            }
    }

    private void AddAnimation(uint hash, BinTreeStructure clipData)
    {
        var mAnimationResourceData =
            ((BinTreeEmbedded) clipData.Properties.FirstOrDefault(property => property.NameHash == 3030349134))
            ?.Properties;
        var mAnimationFilePath =
            ((BinTreeString) mAnimationResourceData?.FirstOrDefault(property => property.NameHash == 53080535))
            ?.Value;
        if (!_parent.EntryExists(mAnimationFilePath))
            return;
        var name = (HashTables.HashTables.BinHashes.ContainsKey(hash)
                       ? HashTables.HashTables.BinHashes[hash]
                       : Path.GetFileName(mAnimationFilePath)) ??
                   hash.ToString();
        _animationFiles[name] = mAnimationFilePath;
    }

    private void ParseStaticMaterialDef(BinTreeObject staticMaterialDef)
    {
        var samplerValues =
            (BinTreeContainer) staticMaterialDef.Properties.FirstOrDefault(property =>
                property.NameHash == 175050421);
        if (samplerValues == null)
            return;
        string texture = null;
        foreach (var staticMaterialShaderSamplerDef in samplerValues.Properties.Cast<BinTreeEmbedded>())
        {
            if (!ParseStaticMaterialShaderSamplerDef(staticMaterialShaderSamplerDef, out var possibleTexture))
                continue;
            texture = possibleTexture;
            break;
        }

        if (_material == staticMaterialDef.PathHash)
            _texture = texture;
        foreach (var material in _materials.Where(material => material.Hash == staticMaterialDef.PathHash))
            material.Texture = texture;
    }

    private static bool ParseStaticMaterialShaderSamplerDef(BinTreeStructure staticMaterialShaderSamplerDef,
        out string texture)
    {
        texture = null;
        var diffuse = false;
        foreach (var property in staticMaterialShaderSamplerDef.Properties)
            switch (property.NameHash)
            {
                case 48757580: // samplerName
                    diffuse = Samplers.Diffuse.Contains(((BinTreeString) property).Value);
                    break;
                case 3004290287: // texture
                    texture = ((BinTreeString) property).Value;
                    break;
            }

        return diffuse;
    }
}