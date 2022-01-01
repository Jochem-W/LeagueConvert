using System.IO;

namespace LeagueToolkit.IO.LightGrid;

public class LightGridSun
{
    public LightGridSun(BinaryReader br)
    {
        OpcaityOfLightOnCharacters = br.ReadSingle();
        Unknown1 = br.ReadSingle();
        Unknown2 = br.ReadSingle();
        Unknown3 = br.ReadSingle();
        Unknown4 = br.ReadSingle();
        Unknown5 = br.ReadSingle();
        Unknown6 = br.ReadSingle();
        Unknown7 = br.ReadSingle();
        Unknown8 = br.ReadSingle();
        Unknown9 = br.ReadSingle();
        Unknown10 = br.ReadSingle();
        Unknown11 = br.ReadSingle();
        Unknown12 = br.ReadSingle();
    }

    public float OpcaityOfLightOnCharacters { get; set; } //What the fuck did I just write
    public float Unknown1 { get; set; }
    public float Unknown2 { get; set; }
    public float Unknown3 { get; set; }
    public float Unknown4 { get; set; }
    public float Unknown5 { get; set; }
    public float Unknown6 { get; set; }
    public float Unknown7 { get; set; }
    public float Unknown8 { get; set; }
    public float Unknown9 { get; set; }
    public float Unknown10 { get; set; }
    public float Unknown11 { get; set; }
    public float Unknown12 { get; set; }

    public void Write(BinaryWriter bw)
    {
        bw.Write(OpcaityOfLightOnCharacters);
        bw.Write(Unknown1);
        bw.Write(Unknown2);
        bw.Write(Unknown3);
        bw.Write(Unknown4);
        bw.Write(Unknown5);
        bw.Write(Unknown6);
        bw.Write(Unknown7);
        bw.Write(Unknown8);
        bw.Write(Unknown9);
        bw.Write(Unknown10);
        bw.Write(Unknown11);
        bw.Write(Unknown12);
    }
}