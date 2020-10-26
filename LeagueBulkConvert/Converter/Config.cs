using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LeagueBulkConvert.Converter
{
    class Config
    {
        public IList<string> ExtractFormats { get; set; }

        public IList<string> IgnoreCharacters { get; set; }

        public IDictionary<string, IDictionary<string, IList<string>>> IgnoreMeshes { get; set; }

        public IList<string> IncludeOnly { get; set; }

        public IList<string> SamplerNames { get; set; }

        [JsonIgnore]
        public float Scale { get; private set; }

        public IList<float> ScaleList { get; set; }

        public void CalculateScale()
        {
            var sum = 0f;
            foreach (var scale in ScaleList)
                sum += scale;
            Scale = sum / ScaleList.Count;
        }
    }
}
