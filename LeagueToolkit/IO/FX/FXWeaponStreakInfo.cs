using System.Text;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.FX;

public class FXWeaponStreakInfo
{
    public FXWeaponStreakInfo(BinaryReader br)
    {
        LinkType = br.ReadInt32();
        BlendType = br.ReadInt32();
        TrailsPerSecond = br.ReadSingle();
        TrailCount = br.ReadSingle();
        StartAlpha = br.ReadSingle();
        EndAlpha = br.ReadSingle();
        AlphaDecay = br.ReadSingle();
        TextureMapMode = br.ReadInt32();

        Texture = Encoding.ASCII.GetString(br.ReadBytes(64));
        Texture = Texture.Remove(Texture.IndexOf(Texture.Contains("\0") ? '\u0000' : '?'));

        ColorOverTime = new TimeGradient(br);
        WidthOverTime = new TimeGradient(br);
    }

    public int LinkType { get; }
    public int BlendType { get; }
    public float TrailsPerSecond { get; }
    public float TrailCount { get; }
    public float StartAlpha { get; }
    public float EndAlpha { get; }
    public float AlphaDecay { get; }
    public int TextureMapMode { get; }
    public string Texture { get; }
    public TimeGradient ColorOverTime { get; }
    public TimeGradient WidthOverTime { get; }

    public void Write(BinaryWriter bw)
    {
        bw.Write(LinkType);
        bw.Write(BlendType);
        bw.Write(TrailsPerSecond);
        bw.Write(TrailCount);
        bw.Write(StartAlpha);
        bw.Write(EndAlpha);
        bw.Write(AlphaDecay);
        bw.Write(TextureMapMode);
        bw.Write(Texture.PadRight(64, '\u0000').ToCharArray());
        ColorOverTime.Write(bw);
        WidthOverTime.Write(bw);
    }
}