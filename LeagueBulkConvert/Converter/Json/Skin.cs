using System.Collections.Generic;

namespace LeagueBulkConvert.Converter.Json
{
    class Skin
    {
        public string Name { get; set; }

        public string Key { get; set; }

        public IList<Chroma> Chromas { get; set; }
    }
}
