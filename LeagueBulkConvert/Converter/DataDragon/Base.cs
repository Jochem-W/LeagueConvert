using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LeagueBulkConvert.Converter.DataDragon
{
    class Base
    {
        [JsonPropertyName("data")]
        public IDictionary<string, Champion> Champions { get; set; }
    }
}
