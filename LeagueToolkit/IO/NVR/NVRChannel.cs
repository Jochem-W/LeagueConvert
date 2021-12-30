using System.IO;
using System.Text;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.NVR;

public class NVRChannel
{
    public NVRChannel(BinaryReader br)
    {
        Color = br.ReadColor(ColorFormat.RgbaF32);
        Name = Encoding.ASCII.GetString(br.ReadBytes(260)).Replace("\0", "");
        Matrix = new R3DMatrix44(br);
    }

    public NVRChannel(string name, Color color, R3DMatrix44 matrix)
    {
        Name = name;
        Color = color;
        Matrix = matrix;
    }

    public Color Color { get; }
    public string Name { get; }
    public R3DMatrix44 Matrix { get; }

    public void Write(BinaryWriter bw)
    {
        bw.WriteColor(Color, ColorFormat.RgbaF32);
        bw.Write(Name.PadRight(260, '\u0000').ToCharArray());
        Matrix.Write(bw);
    }
}