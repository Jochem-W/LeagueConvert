using System.Collections.Generic;

namespace LeagueBulkConvert.Converter.Json
{
    class Champion
    {
        public string Name { get; set; }

        public string Folder { get; set; }

        public IList<Skin> Skins { get; set; }

        public Champion(string name, string folder)
        {
            Name = name;
            Folder = folder;
            Skins = new List<Skin>();
        }
    }
}
