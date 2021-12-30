using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LeagueBulkConvert;

public class Config
{
    private bool _includeAnimations;

    private bool _includeSkeletons;

    private IReadOnlyList<float> _scaleList = new List<float> {0.012389f, 0.013202f, 0.011622f};

    public Config()
    {
        CalculateScale();
    }

    public HashSet<string> ExtractFormats { get; } = new() {".dds", ".skn", ".tga"};

    public bool IncludeAnimations
    {
        get => _includeAnimations;
        set
        {
            _includeAnimations = value;
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

    public bool IncludeHiddenMeshes { get; set; }

    public bool IncludeSkeleton
    {
        get => _includeSkeletons;
        set
        {
            _includeSkeletons = value;
            if (value)
                ExtractFormats.Add(".skl");
            else
                ExtractFormats.Add(".skl");
        }
    }

    public HashSet<string> SamplerNames { get; } = new()
    {
        "Color_Texture",
        "Diff_Tex",
        "Diffuse",
        "DiffuseTexture",
        "Diffuse_Color",
        "Diffuse_Sword_Texture",
        "Diffuse_Texture",
        "Diffuse_Texture_Primary",
        "Main_Texture"
    };

    public bool SaveAsGlTf { get; set; }

    public Matrix4x4 ScaleMatrix { get; private set; }

    public IReadOnlyList<float> ScaleList
    {
        get => _scaleList;
        set
        {
            _scaleList = value;
            CalculateScale();
        }
    }

    public IList<IncludableWad> Wads { get; } = new List<IncludableWad>();

    public List<string> HashTableFiles { get; set; }

    private void CalculateScale()
    {
        var scale = ScaleList.Sum() / ScaleList.Count;
        ScaleMatrix = new Matrix4x4(scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, 1);
    }
}