using LeagueBulkConvert.Converter.Comparers;
using LeagueBulkConvert.MVVM.ViewModels;
using LeagueBulkConvert.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeagueBulkConvert.Converter.Json
{
    static class Utils
    {
        public static async Task CheckColours()
        {
            var fileStream = File.OpenRead("colours.min.json");
            var coloursIn = await JsonSerializer.DeserializeAsync<IDictionary<string, IList<IList<string>>>>(fileStream);
            await fileStream.DisposeAsync();
            var translatedColours = new List<IList<string>>();
            foreach (var colours in coloursIn.Values)
                foreach (var colour in colours)
                    translatedColours.Add(colour);
            fileStream = File.OpenRead("skins.json");
            var cDragon = await JsonSerializer.DeserializeAsync<Dictionary<string, CommunityDragon.Skin>>(fileStream, Converter.SerializerOptions);
            await fileStream.DisposeAsync();
            foreach (var skin in cDragon.Values.Where(s => !(s.Chromas is null)))
                foreach (var chroma in skin.Chromas)
                {
                    if (translatedColours.FindIndex(c => c[0] == chroma.Colours[0] && c[1] == chroma.Colours[1]) != -1)
                        continue;
                    if (chroma.Colours[0] != chroma.Colours[1]) // this will exclude some chromas
                        continue;
                    var promptViewModel = new PromptViewModel
                    {
                        Hint = "Chroma name",
                        Message = chroma.Colours[0],
                        Title = "Enter chroma name"
                    };
                    new PromptWindow { DataContext = promptViewModel }.ShowDialog();
                    if (string.IsNullOrEmpty(promptViewModel.Entry))
                        return;
                    if (coloursIn.ContainsKey(promptViewModel.Entry))
                        coloursIn[promptViewModel.Entry].Add(chroma.Colours);
                    else
                        coloursIn[promptViewModel.Entry] = new List<IList<string>> { chroma.Colours };
                    translatedColours.Add(chroma.Colours);
                }
            fileStream = File.Create("colours.min.json");
            await JsonSerializer.SerializeAsync(fileStream, coloursIn, Converter.SerializerOptions);
            await fileStream.DisposeAsync();
        }

        public static async Task Export()
        {
            var champions = new List<Champion>();
            var fileStream = File.OpenRead("championFull.json");
            var dDragon = (await JsonSerializer.DeserializeAsync<DataDragon.Base>(fileStream, Converter.SerializerOptions)).Champions;
            await fileStream.DisposeAsync();
            fileStream = File.OpenRead("skins.json");
            var cDragon = await JsonSerializer.DeserializeAsync<Dictionary<string, CommunityDragon.Skin>>(fileStream, Converter.SerializerOptions);
            await fileStream.DisposeAsync();
            fileStream = File.OpenRead("colours.min.json");
            var colours = await JsonSerializer.DeserializeAsync<Dictionary<string, List<List<string>>>>(fileStream);
            await fileStream.DisposeAsync();
            foreach ((var id, var dDragonChampion) in dDragon)
            {
                var champion = new Champion(dDragonChampion.Name, id.ToLower());
                foreach (var skinKey in cDragon.Keys.Where(k => k.Remove(k.Length - 3) == dDragonChampion.Key))
                {
                    var cDragonSkin = cDragon[skinKey];
                    var skin = new Skin { Name = cDragonSkin.Name, Key = SimplifyKey(skinKey) };
                    if (cDragonSkin.IsBase)
                        skin.Name = $"Original {skin.Name}";
                    if (!(cDragonSkin.Chromas is null))
                    {
                        skin.Chromas = new List<Chroma> { new Chroma { Key = SimplifyKey(skinKey), Name = "Default" } };
                        for (var i = 0; i < cDragonSkin.Chromas.Count; i++)
                        {
                            var cDragonChroma = cDragonSkin.Chromas[i];
                            var chroma = new Chroma { Key = SimplifyKey(cDragonChroma.Id) };
                            var colour = colours.FirstOrDefault(c =>
                            {
                                foreach (var colour in c.Value)
                                    if (colour[0] == cDragonChroma.Colours[0] && colour[1] == cDragonChroma.Colours[1])
                                        return true;
                                return false;
                            }).Key;
                            if (!string.IsNullOrEmpty(colour))
                                chroma.Name = $"{colour}";
                            else
                                chroma.Name = $"Chroma {i}";
                            skin.Chromas.Add(chroma);
                        }
                        skin.Chromas = skin.Chromas.OrderBy(c => c.Name, new ChromaComparer()).ToList();
                    }
                    champion.Skins.Add(skin);
                }
                champion.Skins = champion.Skins.OrderBy(s => s.Name, new SkinComparer()).ToList();
                champions.Add(champion);
            }
            fileStream = File.Create("skins.min.json");
            await JsonSerializer.SerializeAsync(fileStream, champions.OrderBy(c => c.Name), Converter.SerializerOptions);
            await fileStream.DisposeAsync();
        }

        public static string SimplifyKey(string key) => key.Substring(key.Length - 3).TrimStart('0').PadLeft(1, '0');

        public static string SimplifyKey(int key) => SimplifyKey(key.ToString());
    }
}
