using System.Diagnostics;
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
    private readonly StringWad _parent;
    private readonly Dictionary<string, string> _animationFiles = new();
    private uint? _material;
    private string _simpleSkinFile;
    private string _skeletonFile;
    private string _texture;

    internal Skin(string character, string name, ILogger logger = null, params ParentedBinTree[] binTrees)
    {
        logger?.Debug("Parsing {Character} skin{Id}", Character, Id);
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
    public IList<string> HiddenSubMeshes { get; private set; }
    public IList<MaterialOverride> MaterialOverrides { get; } = new List<MaterialOverride>();
    public IDictionary<uint, StaticMaterial> StaticMaterials { get; } = new Dictionary<uint, StaticMaterial>();
    public SimpleSkin SimpleSkin { get; private set; }
    public IMagickImage Texture { get; private set; }
    public IDictionary<string, IMagickImage> Textures { get; private set; }
    public Skeleton Skeleton { get; private set; }
    public IDictionary<string, Animation> Animations { get; private set; }

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
        if (!State.HasFlagFast(SkinState.MeshLoaded))
            if (!await TryLoadMesh(logger))
                return;
        if (!State.HasFlagFast(SkinState.TexturesLoaded))
            await TryLoadTextures(logger);
        if (mode == SkinMode.MeshAndTextures)
            return;

        if (!State.HasFlagFast(SkinState.SkeletonLoaded))
            await TryLoadSkeleton(logger);
        if (mode == SkinMode.WithSkeleton || !State.HasFlagFast(SkinState.SkeletonLoaded) ||
            State.HasFlagFast(SkinState.AnimationsLoaded))
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
        SetDefaultTexture();
        Textures = new Dictionary<string, IMagickImage>();
        var images = new Dictionary<string, IMagickImage>();
        foreach (var subMesh in SimpleSkin.SubMeshes)
            try
            {
                var texture = FindTexture(subMesh.Name);
                if (texture == null) continue;
                
                if (!images.ContainsKey(texture))
                {
                    await using var stream = _parent.GetEntryByName(texture).GetStream();
                    images[texture] = new MagickImage(stream);
                }

                Textures[subMesh.Name] = images[texture];
            }
            catch (Exception e)
            {
                logger?.Warning(e, "Unexpected error when loading textures: {Exception}", e);
                return false;
            }

        State |= SkinState.TexturesLoaded;
        return true;
    }

    private void SetDefaultTexture()
    {
        if (!_material.HasValue) return;

        string newTexture = null;
        foreach (var materialOverride in MaterialOverrides)
        {
            if (materialOverride.Hash != _material.Value) continue;
            newTexture = materialOverride.Texture;
            break;
        }

        if (newTexture == null && StaticMaterials.ContainsKey(_material.Value))
            StaticMaterials[_material.Value].Samplers.TryGetValue(SamplerType.Diffuse, out newTexture);

        if (newTexture != null) _texture = newTexture;
    }

    private string FindTexture(string subMeshName)
    {
        foreach (var materialOverride in MaterialOverrides)
        {
            if (!StringComparer.InvariantCultureIgnoreCase.Equals(materialOverride.SubMesh, subMeshName)) continue;
            if (materialOverride.Texture != null) return materialOverride.Texture;
            if (!materialOverride.Hash.HasValue) continue;
            if (!StaticMaterials.TryGetValue(materialOverride.Hash.Value, out var staticMaterial)) continue;
            if (!staticMaterial.Samplers.TryGetValue(SamplerType.Diffuse, out var texture)) continue;
            if (texture != null) return texture;
        }

        return _texture;
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

    private void ParseBinTree(BinTree binTree)
    {
        foreach (var binTreeObject in binTree.Objects)
            switch (binTreeObject.MetaClassHash)
            {
                case 2607278582: // SkinCharacterDataProperties
                    ParseSkinCharacterDataProperties(binTreeObject);
                    break;
                case 4126869447: // TODO
                    ParseAnimationGraphData(binTreeObject);
                    break;
                case 4288492553: // StaticMaterialDef
                    ParseStaticMaterialDef(binTreeObject);
                    break;
                // TODO: MaterialInstanceDef?
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
                            _material = ((BinTreeObjectLink) property).Value;
                            break;
                        case 2159540111: // initialSubMeshToHide
                            var initialSubMeshToHide = ((BinTreeString) property).Value;
                            HiddenSubMeshes = initialSubMeshToHide.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            break;
                        case 611473680: // materialOverride
                            foreach (var materialOverride in ((BinTreeContainer) property).Properties)
                                AddMaterialOverride((BinTreeEmbedded) materialOverride);
                            break;
                    }

                break;
        }
    }

    private void AddMaterialOverride(BinTreeStructure skinMeshDataPropertiesMaterialOverride)
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

        MaterialOverrides.Add(new MaterialOverride(materialLink, texture, subMesh));
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
        Debug.Assert(!StaticMaterials.ContainsKey(staticMaterialDef.PathHash));

        var staticMaterial = new StaticMaterial(staticMaterialDef.PathHash);
        foreach (var property in staticMaterialDef.Properties)
            switch (property.NameHash)
            {
                case 175050421: // samplerValues
                    var samplerValues = (BinTreeContainer) property;
                    foreach (var binTreeProperty in samplerValues.Properties)
                    {
                        var samplerValue = (BinTreeEmbedded) binTreeProperty;
                        ParseStaticMaterialShaderSamplerDef(staticMaterial, samplerValue);
                    }

                    break;
            }

        StaticMaterials[staticMaterialDef.PathHash] = staticMaterial;
    }

    private static void ParseStaticMaterialShaderSamplerDef(StaticMaterial staticMaterial,
        BinTreeEmbedded staticMaterialShaderSamplerDef)
    {
        var samplerType = SamplerType.Unknown;
        string texture = null;
        foreach (var property in staticMaterialShaderSamplerDef.Properties)
            switch (property.NameHash)
            {
                case 48757580: // samplerName
                    samplerType = Samplers.FromString(((BinTreeString) property).Value);
                    break;
                case 3004290287: // texture
                    texture = ((BinTreeString) property).Value;
                    break;
            }

        if (samplerType == SamplerType.Unknown) return;

        Debug.Assert(!staticMaterial.Samplers.ContainsKey(samplerType));

        staticMaterial.Samplers[samplerType] = texture;
    }
}