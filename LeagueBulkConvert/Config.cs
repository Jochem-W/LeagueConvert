using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LeagueBulkConvert
{
    public class Config
    {
        public HashSet<string> ExtractFormats { get; } = new HashSet<string> { ".dds", ".jpg", ".png", ".skn", ".tga" };

        private bool includeAnimations = false;
        public bool IncludeAnimations
        {
            get => includeAnimations;
            set
            {
                includeAnimations = value;
                if (value)
                {
                    IncludeSkeleton = true;
                    ExtractFormats.Add(".anm");
                    ExtractFormats.Add(".bin");
                }
                else
                {
                    ExtractFormats.Remove(".anm");
                    ExtractFormats.Remove(".bin");
                }
            }
        }

        public bool IncludeHiddenMeshes { get; set; } = false;

        private bool includeSkeletons = false;
        public bool IncludeSkeleton
        {
            get => includeSkeletons;
            set
            {
                includeSkeletons = value;
                if (value)
                    ExtractFormats.Add(".skl");
                else
                    ExtractFormats.Add(".skl");
            }
        }

        public bool ReadVersion3 { get; set; } = true;

        public HashSet<string> SamplerNames { get; } = new HashSet<string>
        {
            "Diffuse_Texture",
            "DiffuseTexture",
            "Main_Texture",
            "Diffuse_Sword_Texture",
            "Color_Texture",
            "Diffuse_Color"
        };

        public bool SaveAsGlTF { get; set; } = false;

        public Matrix4x4 ScaleMatrix { get; private set; }

        private IReadOnlyList<float> scaleList = new List<float> { 0.012389f, 0.013202f, 0.011622f };

        public IReadOnlyList<float> ScaleList
        {
            get => scaleList;
            set
            {
                scaleList = value;
                CalculateScale();
            }
        }

        public IList<IncludableWad> Wads { get; } = new List<IncludableWad>();

        private void CalculateScale()
        {
            var scale = ScaleList.Sum() / ScaleList.Count;
            ScaleMatrix = new Matrix4x4(scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, 1);
        }

        public Config() => CalculateScale();
    }
}
