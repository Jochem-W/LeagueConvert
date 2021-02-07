using ImageMagick;
using LeagueToolkit.IO.AnimationFile;
using LeagueToolkit.IO.PropertyBin;
using LeagueToolkit.IO.PropertyBin.Properties;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.IO.SkeletonFile;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeagueBulkConvert
{
    class Skin
    {
        public List<(string, Animation)> Animations { get; } = new List<(string, Animation)>();

        public string Character { get; }

        public bool Exists { get => File.Exists(Mesh); }

        public ulong MaterialHash { get; private set; }

        public IList<Material> Materials { get; } = new List<Material>();

        public string Mesh { get; private set; }

        public string Name { get; }

        public IList<string> HiddenMeshes { get; } = new List<string>();

        public string Skeleton { get; private set; }

        public string Texture { get; private set; }

        public async Task AddAnimations(string binPath, IDictionary<string, IDictionary<ulong, string>> hashTables, Config config,
            ILogger logger = null, CancellationToken? cancellationToken = null)
        {
            BinTree binTree;
            if (config.ReadVersion3)
                binTree = await Utils.ReadVersion3(binPath);
            else
                binTree = new BinTree(binPath);
            if (binTree.Objects.Count != 1)
                throw new NotImplementedException();
            var animations = (BinTreeMap)binTree.Objects[0].Properties.FirstOrDefault(p => p.NameHash == 1172382456); //mClipDataMap
            if (animations == null)
                return;
            foreach (var keyValuePair in animations.Map)
            {
                if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                    return;
                var structure = (BinTreeStructure)keyValuePair.Value;
                if (structure.MetaClassHash != 1540989414) //AtomicClipData
                    continue;
                var animationData = (BinTreeEmbedded)structure.Properties.FirstOrDefault(p => p.NameHash == 3030349134); //mAnimationResourceData
                if (animationData == null)
                    continue;
                var pathProperty = (BinTreeString)animationData.Properties.FirstOrDefault(p => p.NameHash == 53080535); //mAnimationFilePath
                if (pathProperty == null)
                    continue;
                var path = pathProperty.Value.ToString().ToLower();
                if (!File.Exists(path))
                    continue;
                var hash = ((BinTreeHash)keyValuePair.Key).Value;
                string name;
                if (hashTables["binhashes"].ContainsKey(hash))
                    name = hashTables["binhashes"][hash];
                else
                    name = Path.GetFileNameWithoutExtension(path);
                Animation animation;
                try
                {
                    animation = new Animation(path);
                }
                catch (Exception)
                {
                    if (logger != null)
                        logger.Information($"    Couldn't parse {path}");
                    continue;
                }
                Animations.Add((name, animation));
            }
        }

        public void Clean()
        {
            var baseMaterial = Materials.FirstOrDefault(m => m.Hash == MaterialHash);
            if (baseMaterial != null && Texture != null && !baseMaterial.IsComplete)
                Materials[Materials.IndexOf(baseMaterial)].Texture = Texture;
            for (var i = 0; i < Materials.Count; i++)
            {
                var material = Materials[i];
                if (material.Hash == 0 && string.IsNullOrWhiteSpace(material.Texture) && !HiddenMeshes.Contains(material.Name))
                    Materials[i].Texture = Texture;
                if ((string.IsNullOrWhiteSpace(material.Texture)
                     || material.Texture.Contains("empty32.dds"))
                     && !HiddenMeshes.Contains(material.Name))
                    HiddenMeshes.Add(material.Name);
                if (!HiddenMeshes.Contains(material.Name))
                    continue;
                Materials.RemoveAt(i);
                i--;
            }
        }

        private void ParseBinTree(BinTree tree, Config config)
        {
            foreach (var binTreeObject in tree.Objects)
                ParseBinTreeObject(binTreeObject, config);
        }

        private void ParseBinTreeContainer(BinTreeContainer container)
        {
            foreach (var property in container.Properties)
                ParseBinTreeEmbedded((BinTreeEmbedded)property);
        }

        private void ParseBinTreeEmbedded(BinTreeEmbedded tree)
        {
            switch (tree.MetaClassHash)
            {
                case 1628559524: //SkinMeshDataProperties
                    foreach (var property in tree.Properties)
                        ParseBinTreeProperty(property);
                    break;
                case 2340045716: //SkinMeshDataProperties_MaterialOverride
                    var materialProperty = tree.Properties.FirstOrDefault(p => p.NameHash == 3538210912); //material
                    var submeshProperty = tree.Properties.FirstOrDefault(p => p.NameHash == 2866241836); //submesh
                    var textureProperty = tree.Properties.FirstOrDefault(p => p.NameHash == 1013213428); //texture
                    Materials.Add(new Material(materialProperty, submeshProperty, textureProperty));
                    break;
            }

        }

        private void ParseBinTreeObject(BinTreeObject treeObject, Config config)
        {
            switch (treeObject.MetaClassHash)
            {
                case 2607278582: //SkinCharacterDataProperties
                    foreach (var property in treeObject.Properties)
                        ParseBinTreeProperty(property);
                    break;
                // the following code is kind of weird
                case 4288492553: //StaticMaterialDef
                    if (treeObject.PathHash == MaterialHash)
                    {
                        if (Utils.FindTexture(treeObject, config, out var texture))
                            Texture = texture;
                    }
                    foreach (var material in Materials.Where(m => m.Hash == treeObject.PathHash && !m.IsComplete))
                        material.Complete(treeObject, config);
                    break;
            }
        }

        private void ParseBinTreeProperty(BinTreeProperty property)
        {
            switch (property.NameHash)
            {
                case 1174362372: //skinMeshProperties
                    ParseBinTreeEmbedded((BinTreeEmbedded)property);
                    break;
                case 2974586734: //skeleton
                    Skeleton = ((BinTreeString)property).Value.ToLower();
                    break;
                case 3600813558: //simpleSkin
                    Mesh = ((BinTreeString)property).Value.ToLower();
                    break;
                case 1013213428: //texture
                    Texture = ((BinTreeString)property).Value.ToLower();
                    break;
                case 2159540111: //initialSubmeshToHide
                    foreach (var submesh in ((BinTreeString)property).Value.Replace(',', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries))
                        HiddenMeshes.Add(submesh.ToLower());
                    break;
                case 611473680: //materialOverride
                    ParseBinTreeContainer((BinTreeContainer)property);
                    break;
                case 3538210912: //material
                    if (((BinTreeEmbedded)property.Parent).MetaClassHash != 1628559524) //SkinMeshDataProperties
                        throw new NotImplementedException();
                    MaterialHash = ((BinTreeObjectLink)property).Value;
                    break;
            }
        }

        public void Save(Config config, ILogger logger = null)
        {
            if (!File.Exists(Mesh))
                return;
            SimpleSkin simpleSkin;
            try
            {
                simpleSkin = new SimpleSkin(Mesh);
            }
            catch (Exception)
            {
                if (logger != null)
                    logger.Information($"    Couldn't parse {Mesh}");
                return;
            }
            var materialTextures = new Dictionary<string, MagickImage>();
            IDictionary<string, MagickImage> textures = new Dictionary<string, MagickImage>();
            for (var i = 0; i < simpleSkin.Submeshes.Count; i++)
            {
                var submesh = simpleSkin.Submeshes[i];
                if (HiddenMeshes.Contains(submesh.Name.ToLower()))
                {
                    simpleSkin.Submeshes.RemoveAt(i);
                    i--;
                    continue;
                }
                var material = Materials.FirstOrDefault(m => m.Name == submesh.Name.ToLower());
                string textureFile;
                if (material == null)
                    textureFile = Texture;
                else
                    textureFile = material.Texture;
                if (!textures.ContainsKey(textureFile))
                    textures[textureFile] = new MagickImage(textureFile);
                materialTextures[submesh.Name] = textures[textureFile];
            }
            SharpGLTF.Schema2.ModelRoot gltf;
            if (!config.IncludeSkeleton)
            {
                gltf = simpleSkin.ToGltf(materialTextures);
                gltf.ApplyBasisTransform(config.ScaleMatrix);
            }
            else
            {
                Skeleton skeleton;
                try
                {
                    skeleton = new Skeleton(Skeleton);
                }
                catch (Exception)
                {
                    if (logger != null)
                        logger.Information($"    Couldn't parse {Skeleton}");
                    return;
                }
                if (Animations == null)
                    gltf = simpleSkin.ToGltf(skeleton, materialTextures);
                else
                    gltf = simpleSkin.ToGltf(skeleton, materialTextures, Animations);
            }
            var directory = $"export/{Character}";
            string exportPath;
            if (config.SaveAsGlTF)
            {
                directory += $"/{Name}";
                exportPath = $"{directory}/{Name}.gltf";
            }
            else
                exportPath = $"{directory}/{Name}.glb";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            gltf.Save(exportPath);
        }

        public Skin(string character, string name, BinTree tree, Config config)
        {
            Character = character;
            Name = name;
            ParseBinTree(tree, config);
            if (config.IncludeHiddenMeshes)
                HiddenMeshes.Clear();
        }
    }
}