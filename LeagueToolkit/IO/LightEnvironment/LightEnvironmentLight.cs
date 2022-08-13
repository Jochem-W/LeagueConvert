using System.Globalization;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.LightEnvironment;

public class LightEnvironmentLight
{
    public LightEnvironmentLight(int[] position, Color color, Color color2, int unknown1, int unknown2, bool unknown3,
        float opacity)
    {
        Position = position;
        Color = color;
        Color2 = color2;
        Unknown1 = unknown1;
        Unknown2 = unknown2;
        Unknown3 = unknown3;
        Opacity = opacity;
    }

    public LightEnvironmentLight(StreamReader sr)
    {
        var line = sr.ReadLine().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        Position = new[] { int.Parse(line[0]), int.Parse(line[1]), int.Parse(line[2]) };
        Color = new Color(byte.Parse(line[3]), byte.Parse(line[4]), byte.Parse(line[5]));
        Color2 = new Color(byte.Parse(line[6]), byte.Parse(line[7]), byte.Parse(line[8]));
        Unknown1 = int.Parse(line[9]);
        Unknown2 = int.Parse(line[10]);
        Unknown3 = line[11] == "1";
        Opacity = float.Parse(line[12], CultureInfo.InvariantCulture);
    }

    public int[] Position { get; } //Decimal Integer position, not float
    public Color Color { get; } //*(v6 + 67) = (v38 << 16) | (v37 << 8) | v36 | 0xFF000000;

    public Color
        Color2
    {
        get;
    } //Not so sure about this one, seems to be a color *(v6 + 67) = (v38 << 16) | (v37 << 8) | v36 | 0xFF000000;

    public int Unknown1 { get; } //Seems to be a flag
    public int Unknown2 { get; }
    public bool Unknown3 { get; }

    public float
        Opacity
    {
        get;
    } //No idea what this might be but probably Opacity of the light since there is no alpha channel in color

    public void Write(StreamWriter sw)
    {
        sw.Write("{0} {1} {2} ", Position[0], Position[1], Position[2]);
        sw.WriteColor(Color, ColorFormat.RgbU8);
        sw.Write(" ");
        sw.WriteColor(Color2, ColorFormat.RgbU8);
        sw.Write(" ");
        sw.Write("{0} {1} {2} {3}" + Environment.NewLine, Unknown1, Unknown2, Convert.ToUInt16(Unknown3), Opacity);
    }
}
/*
[Flags]
public enum LightFlags : UInt32
{
    R3D_LIGHT_ON = 2,
    R3D_LIGHT_STATIC = 4,
    R3D_LIGHT_DYNAMIC = 8,
    R3D_LIGHT_HEAP = 16,
    R3D_LIGHT_AUTOREMOVE = 32,
    R3D_LIGHT_ALWAYSVISIBLE = 64
}

public enum LightType : UInt32
{
    R3D_OMNI_LIGHT = 0,
    R3D_DIRECT_LIGHT = 1,
    R3D_SPOT_LIGHT = 2,
    R3D_PROJECTOR_LIGHT = 3,
    R3D_CUBE_LIGHT = 4
}
*/