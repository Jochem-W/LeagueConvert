using System.Globalization;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.MaterialLibrary;

public class MaterialLibraryMaterial
{
    public MaterialLibraryMaterial(string name, MaterialFlags flags, Color emissiveColor, float[] uvScroll,
        bool isBackfaceCullingDisabled,
        string shaderName, bool isSimpleShader, byte opacity, Color color)
    {
        Name = name;
        Flags = flags;
        EmissiveColor = emissiveColor;
        UVScroll = uvScroll;
        IsBackfaceCullingDisabled = isBackfaceCullingDisabled;
        ShaderName = shaderName;
        IsSimpleShader = isSimpleShader;
        Opacity = opacity;
        Color = color;
    }

    public MaterialLibraryMaterial(string name, MaterialFlags flags, Color emissiveColor, float[] uvScroll,
        bool isBackfaceCullingDisabled,
        string shaderName, bool isSimpleShader, byte opacity, Color color, string texture)
    {
        Name = name;
        Flags = flags;
        EmissiveColor = emissiveColor;
        UVScroll = uvScroll;
        IsBackfaceCullingDisabled = isBackfaceCullingDisabled;
        ShaderName = shaderName;
        IsSimpleShader = isSimpleShader;
        Opacity = opacity;
        Texture = texture;
        Color = color;
    }

    public MaterialLibraryMaterial(StreamReader sr)
    {
        string[] line;
        while ((line = sr.ReadLine().Split(new[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries))[0] !=
               "[MaterialEnd]")
        {
            if (line[0] == "Name")
            {
                Name = line[1];
            }
            else if (line[0] == "Flags")
            {
                if (line.Contains("addop"))
                {
                    Flags |= MaterialFlags.AddOp;
                }

                if (line.Contains("subop"))
                {
                    Flags |= MaterialFlags.SubOp;
                }

                if (line.Contains("alphaop"))
                {
                    Flags |= MaterialFlags.AlphaOp;
                }

                if (line.Contains("uvclamp"))
                {
                    Flags |= MaterialFlags.UVClamp;
                }

                if (line.Contains("texture_gouraud_"))
                {
                    Flags |= MaterialFlags.GroundTexture;
                }
            }
            else if (line[0] == "EmissiveColor")
            {
                var r = int.Parse(line[1]);
                var g = int.Parse(line[2]);
                var b = int.Parse(line[3]);
                EmissiveColor = new Color
                (
                    r == int.MinValue ? (byte)~r : (byte)r,
                    g == int.MinValue ? (byte)~g : (byte)g,
                    b == int.MinValue ? (byte)~b : (byte)b
                );
            }
            else if (line[0] == "UVScroll")
            {
                UVScroll = new[]
                {
                    float.Parse(line[1], CultureInfo.InvariantCulture),
                    float.Parse(line[2], CultureInfo.InvariantCulture)
                };
            }
            else if (line[0] == "DisableBackfaceCulling")
            {
                IsBackfaceCullingDisabled = line[1] == "1";
            }
            else if (line[0] == "ShaderName")
            {
                ShaderName = line[1];
            }
            else if (line[0] == "SimpleShader")
            {
                IsSimpleShader = line[1] == "1";
            }
            else if (line[0] == "Opacity")
            {
                Opacity = byte.Parse(line[1]);
            }
            else if (line[0] == "Texture")
            {
                Texture = line[1];
            }
            else if (line[0] == "Color24")
            {
                var r = int.Parse(line[1]);
                var g = int.Parse(line[2]);
                var b = int.Parse(line[3]);
                Color = new Color
                (
                    r == int.MinValue ? (byte)~r : (byte)r,
                    g == int.MinValue ? (byte)~g : (byte)g,
                    b == int.MinValue ? (byte)~b : (byte)b
                );
            }
        }
    }

    public string Name { get; }
    public MaterialFlags Flags { get; }
    public Color EmissiveColor { get; }
    public float[] UVScroll { get; }
    public bool IsBackfaceCullingDisabled { get; }
    public string ShaderName { get; }
    public bool IsSimpleShader { get; }
    public byte Opacity { get; }
    public string Texture { get; }
    public Color Color { get; }

    public void Write(StreamWriter sw)
    {
        var flags = "";
        if (Flags.HasFlag(MaterialFlags.GroundTexture))
        {
            flags += "texture_gouraud_ ";
        }

        if (Flags.HasFlag(MaterialFlags.AddOp))
        {
            flags += "addop ";
        }

        if (Flags.HasFlag(MaterialFlags.SubOp))
        {
            flags += "subop";
        }

        if (Flags.HasFlag(MaterialFlags.AlphaOp))
        {
            flags += "alphaop";
        }

        if (Flags.HasFlag(MaterialFlags.UVClamp))
        {
            flags += "uvclamp";
        }

        sw.WriteLine("[MaterialBegin]");
        sw.WriteLine("Name= " + Name);
        sw.WriteLine("Flags= " + flags);
        sw.WriteLine("EmissiveColor= {0} {1} {2}", EmissiveColor.R, EmissiveColor.G, EmissiveColor.B);
        sw.WriteLine("UVScroll = {0} {1}", UVScroll[0], UVScroll[1]);
        sw.WriteLine("DisableBackfaceCulling = " + (IsBackfaceCullingDisabled ? "1" : "0"));
        sw.WriteLine("ShaderName = " + ShaderName);
        sw.WriteLine("SimpleShader = " + (IsSimpleShader ? "1" : "0"));
        sw.WriteLine("Opacity= " + Opacity);
        if (Texture != null)
        {
            sw.WriteLine("Texture= " + Texture);
        }

        sw.WriteLine("Color24= {0}", Color.ToString(ColorFormat.RgbU8));
        sw.WriteLine("[MaterialEnd]");
    }
}

[Flags]
public enum MaterialFlags
{
    AddOp = 0x100,
    SubOp = 0x200,
    AlphaOp = 0x400,
    UVClamp = 0x100000,
    GroundTexture = 0x200000
}