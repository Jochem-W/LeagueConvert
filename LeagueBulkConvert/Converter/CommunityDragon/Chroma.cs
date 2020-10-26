using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LeagueBulkConvert.Converter.CommunityDragon
{
    class Chroma
    {
        [JsonPropertyName("colors")]
        public IList<string> Colours { get; set; }

        public int Id { get; set; }
    }
}
