using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LeagueBulkConvert.Conversion
{
    class Config
    {
        public IList<string> ExtractFormats { get; set; }

        public IList<string> IgnoreCharacters { get; set; }

        public IDictionary<string, IDictionary<string, IList<string>>> IgnoreMeshes { get; set; }

        public IList<string> IncludeOnly { get; set; }

        public IList<string> SamplerNames { get; set; }

        [JsonIgnore]
        public Matrix4x4 ScaleMatrix { get; private set; }

        public IList<float> ScaleList { get; set; }

        public void CalculateScale()
        {
            var sum = 0f;
            foreach (var scale in ScaleList)
                sum += scale;
            var averageScale = sum / ScaleList.Count;
            ScaleMatrix = new Matrix4x4(averageScale, 0, 0, 0, 0, averageScale, 0, 0, 0, 0, averageScale, 0, 0, 0, 0, 1);
        }
    }
}
