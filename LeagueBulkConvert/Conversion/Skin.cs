using Fantome.Libraries.League.IO.AnimationFile;
using Fantome.Libraries.League.IO.BIN;
using Fantome.Libraries.League.IO.SimpleSkinFile;
using Fantome.Libraries.League.IO.SkeletonFile;
using ImageMagick;
using LeagueBulkConvert.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeagueBulkConvert.Conversion
{
    class Skin
    {
        public List<(string, Animation)> Animations { get; set; }

        public string Character { get; set; }

        public bool Exists { get => File.Exists(Mesh); }

        public ulong MaterialHash { get; set; }

        public IList<Material> Materials { get; set; } = new List<Material>();

        public string Mesh { get; set; }

        public string Name { get; set; }

        public IList<string> RemoveMeshes { get; set; }

        public string Skeleton { get; set; }

        public string Texture { get; set; }

        public void AddAnimations(string binPath, LoggingViewModel viewModel)
        {
            var binFile = new BINFile(binPath);
            if (binFile.Entries.Count != 1)
                throw new NotImplementedException();
            var animationValue = binFile.Entries[0].Values.FirstOrDefault(v => v.Property == 1172382456); //mClipDataMap
            foreach (var value in ((BINMap)animationValue.Value).Values)
            {
                var structure = (BINStructure)value.Value.Value;
                if (structure.Property != 1540989414) // AtomicClipData
                    continue;
                var animationData = structure.Values.FirstOrDefault(v => v.Property == 3030349134); // mAnimationResourceData
                var animationDataStructure = (BINStructure)animationData.Value;
                var pathValue = animationDataStructure.Values.FirstOrDefault(v => v.Property == 53080535); // mAnimationFilePath
                var path = pathValue.Value.ToString().ToLower().Replace('/', '\\');
                if (!File.Exists(path))
                    continue;
                if (!ulong.TryParse(value.Key.Value.ToString(), out ulong key))
                    throw new NotImplementedException();
                string name;
                if (Converter.HashTables["binhashes"].ContainsKey(key))
                    name = Converter.HashTables["binhashes"][key];
                else
                    name = Path.GetFileNameWithoutExtension(path);
                Animation animation;
                try
                {
                    animation = new Animation(path);
                }
                catch (Exception)
                {
                    viewModel.AddLine($"Couldn't parse {path}", 2);
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
                if (material.Hash == 0 && string.IsNullOrWhiteSpace(material.Texture) && !RemoveMeshes.Contains(material.Name))
                    Materials[i].Texture = Texture;
                if ((string.IsNullOrWhiteSpace(material.Texture)
                     || material.Texture.Contains("empty32.dds"))
                     && !RemoveMeshes.Contains(material.Name))
                    RemoveMeshes.Add(material.Name);
                if (!RemoveMeshes.Contains(material.Name))
                    continue;
                Materials.RemoveAt(i);
                i--;
            }
        }

        private void ParseBinEntry(BINEntry entry)
        {
            switch (entry.Class)
            {
                case 2607278582: //SkinCharacterDataProperties
                    foreach (var value in entry.Values)
                        ParseBinValue(value);
                    break;
                case 4288492553: //StaticMaterialDef
                    if (entry.Property == MaterialHash)
                    {
                        var foundTexture = Utils.FindTexture(entry);
                        if (!string.IsNullOrEmpty(foundTexture))
                            Texture = foundTexture;
                    }
                    foreach (var material in Materials.Where(m => m.Hash == entry.Property && !m.IsComplete))
                        Materials[Materials.IndexOf(material)].Complete(entry);
                    break;
            }
        }

        private void ParseBinValue(BINValue value)
        {
            switch (value.Property)
            {
                case 1174362372: //skinMeshProperties
                    ParseBinStructure((BINStructure)value.Value);
                    break;
                case 2974586734: //skeleton
                    Skeleton = ((string)value.Value).ToLower().Replace('/', '\\');
                    break;
                case 3600813558: //simpleSkin
                    Mesh = ((string)value.Value).ToLower().Replace('/', '\\');
                    break;
                case 1013213428: //texture
                    Texture = ((string)value.Value).ToLower().Replace('/', '\\');
                    break;
                case 2159540111: //initialSubmeshToHide
                    foreach (var mesh in ((string)value.Value).Replace(',', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries))
                        RemoveMeshes.Add(mesh.ToLower());
                    break;
                case 611473680: //materialOverride
                    ParseBinContainer((BINContainer)value.Value);
                    break;
                case 3538210912: //material
                    if (((BINStructure)value.Parent).Property == 1628559524) //SkinMeshDataProperties
                        MaterialHash = (uint)value.Value;
                    break;
            }
        }

        private void ParseBinStructure(BINStructure structure)
        {
            switch (structure.Property)
            {
                case 1628559524: //SkinMeshDataProperties
                    foreach (var value in structure.Values)
                        ParseBinValue(value);
                    break;
                case 2340045716: //SkinMeshDataProperties_MaterialOverride
                    var material = structure.Values.FirstOrDefault(v => v.Property == 3538210912); //material
                    var submesh = structure.Values.FirstOrDefault(v => v.Property == 2866241836); //submesh
                    var texture = structure.Values.FirstOrDefault(v => v.Property == 1013213428); //texture
                    Materials.Add(new Material(material, submesh, texture));
                    break;
            }

        }

        private void ParseBinContainer(BINContainer container)
        {
            foreach (var value in container.Values)
                ParseBinStructure((BINStructure)value.Value);
        }

        public void Save(MainViewModel viewModel, LoggingViewModel loggingViewModel)
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
                loggingViewModel.AddLine($"Couldn't parse {Mesh}", 2);
                return;
            }
            var materialTextures = new Dictionary<string, MagickImage>();
            IDictionary<string, MagickImage> textures = new Dictionary<string, MagickImage>();
            for (var i = 0; i < simpleSkin.Submeshes.Count; i++)
            {
                var submesh = simpleSkin.Submeshes[i];
                if (RemoveMeshes.Contains(submesh.Name.ToLower()))
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
            var folderPath = @$"export\{Character}";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            SharpGLTF.Schema2.ModelRoot gltf;
            if (!viewModel.IncludeSkeletons)
            {
                gltf = simpleSkin.ToGltf(materialTextures);
                gltf.ApplyBasisTransform(Converter.Config.ScaleMatrix);
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
                    loggingViewModel.AddLine($"Couldn't parse {Skeleton}", 2);
                    return;
                }
                if (Animations == null)
                    gltf = simpleSkin.ToGltf(skeleton, materialTextures);
                else
                    gltf = simpleSkin.ToGltf(skeleton, materialTextures, Animations);
            }
            gltf.SaveGLB(@$"{folderPath}\{Name}.glb");
        }

        public Skin(string character, string name, BINFile file, MainViewModel viewModel, LoggingViewModel loggingViewModel)
        {
            Character = character;
            Name = name;
            if (!viewModel.IncludeHiddenMeshes
                && Converter.Config.IgnoreMeshes.ContainsKey(character)
                && Converter.Config.IgnoreMeshes[character].ContainsKey(name))
                RemoveMeshes = new List<string>(Converter.Config.IgnoreMeshes[character][name]);
            else
                RemoveMeshes = new List<string>();
            foreach (var entry in file.Entries)
                ParseBinEntry(entry);
            if (viewModel.IncludeHiddenMeshes)
                RemoveMeshes = new List<string>();
            if (viewModel.IncludeAnimations)
            {
                Animations = new List<(string, Animation)>();
                foreach (var filePath in file.Dependencies)
                {
                    var lowercase = filePath.ToLower();
                    if (lowercase.Contains("/animations/") && File.Exists(lowercase))
                        try
                        {
                            AddAnimations(lowercase, loggingViewModel);
                        }
                        catch (Exception)
                        {
                            loggingViewModel.AddLine($"Couldn't add animations", 2);
                            return;
                        }
                }

            }

        }
    }

}
