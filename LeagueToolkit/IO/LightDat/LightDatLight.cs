using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.LightDat;

public class LightDatLight
{
    public LightDatLight(int[] position, Color color, int radius)
    {
        Position = position;
        Color = color;
        Radius = radius;
    }

    public LightDatLight(StreamReader sr)
    {
        var line = sr.ReadLine().Split(' ');
        Position = new[] {int.Parse(line[0]), int.Parse(line[1]), int.Parse(line[2])};
        Color = new Color(byte.Parse(line[3]), byte.Parse(line[4]), byte.Parse(line[5]));
        Radius = int.Parse(line[6]);
    }

    public int[] Position { get; }
    public Color Color { get; }
    public int Radius { get; }

    public void Write(StreamWriter sw)
    {
        sw.Write("{0} {1} {2} ", Position[0], Position[1], Position[2]);
        sw.WriteColor(Color, ColorFormat.RgbU8);
        sw.Write(" ");
        sw.Write(Radius + Environment.NewLine);
    }
}