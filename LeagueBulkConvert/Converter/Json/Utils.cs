using LeagueBulkConvert.Converter.Comparers;
using LeagueBulkConvert.Converter.DataDragon;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeagueBulkConvert.Converter.Json
{
    static class Utils
    {
        public static JsonSerializerOptions SerializerOptions { get; private set; }

        public static async Task Export()
        {
            var champions = new List<Champion>();
            var fileStream = File.OpenRead("championFull.json");
            var dDragon = (await JsonSerializer.DeserializeAsync<Base>(fileStream, SerializerOptions)).Champions;
            await fileStream.DisposeAsync();
            fileStream = File.OpenRead("skins.json");
            var cDragon = await JsonSerializer.DeserializeAsync<Dictionary<string, CommunityDragon.Skin>>(fileStream, SerializerOptions);
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
                    var skin = new Skin { Name = cDragonSkin.Name, Key = LeagueBulkConvert.Converter.Utils.SimplifyKey(skinKey) };
                    if (cDragonSkin.IsBase)
                        skin.Name = $"Original {skin.Name}";
                    if (!(cDragonSkin.Chromas is null))
                    {
                        skin.Chromas = new List<Chroma> { new Chroma { Key = LeagueBulkConvert.Converter.Utils.SimplifyKey(skinKey), Name = "Default" } };
                        for (var i = 0; i < cDragonSkin.Chromas.Count; i++)
                        {
                            var cDragonChroma = cDragonSkin.Chromas[i];
                            var chroma = new Chroma { Key = LeagueBulkConvert.Converter.Utils.SimplifyKey(cDragonChroma.Id) };
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
            //CheckMissing(champions);
            fileStream = File.Create("skins.min.json");
            await JsonSerializer.SerializeAsync(fileStream, champions.OrderBy(c => c.Name), SerializerOptions);
            await fileStream.DisposeAsync();
        }
    }
}
