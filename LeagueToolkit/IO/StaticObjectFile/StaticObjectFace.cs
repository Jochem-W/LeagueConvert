using System.Globalization;
using System.Numerics;
using System.Text;

namespace LeagueToolkit.IO.StaticObjectFile;

internal class StaticObjectFace
{
    public StaticObjectFace(uint[] indices, string material, Vector2[] uvs)
    {
        Indices = indices;
        Material = material;
        UVs = uvs;
    }

    public StaticObjectFace(BinaryReader br)
    {
        Indices = new[] { br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32() };
        Material = Encoding.ASCII.GetString(br.ReadBytes(64)).Replace("\0", "");

        float[] uvs =
            { br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle() };
        UVs = new[]
        {
            new(uvs[0], uvs[3]),
            new Vector2(uvs[1], uvs[4]),
            new Vector2(uvs[2], uvs[5])
        };
    }

    public StaticObjectFace(StreamReader sr)
    {
        var input = sr.ReadLine().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        Indices = new[] { uint.Parse(input[1]), uint.Parse(input[2]), uint.Parse(input[3]) };
        Material = input[4];
        UVs = new[]
        {
            new(float.Parse(input[5], CultureInfo.InvariantCulture),
                float.Parse(input[6], CultureInfo.InvariantCulture)),
            new Vector2(float.Parse(input[7], CultureInfo.InvariantCulture),
                float.Parse(input[8], CultureInfo.InvariantCulture)),
            new Vector2(float.Parse(input[9], CultureInfo.InvariantCulture),
                float.Parse(input[10], CultureInfo.InvariantCulture))
        };
    }

    public uint[] Indices { get; }
    public string Material { get; }
    public Vector2[] UVs { get; }

    public void Write(BinaryWriter bw)
    {
        for (var i = 0; i < 3; i++)
        {
            bw.Write(Indices[i]);
        }

        bw.Write(Material.PadRight(64, '\u0000').ToCharArray());

        for (var i = 0; i < 3; i++)
        {
            bw.Write(UVs[i].X);
        }

        for (var i = 0; i < 3; i++)
        {
            bw.Write(UVs[i].Y);
        }
    }

    public void Write(StreamWriter sw)
    {
        var indices = string.Format("{0} {1} {2}", Indices[0], Indices[1], Indices[2]);
        var uvs = string.Format("{0} {1} {2} {3} {4} {5}",
            UVs[0].X, UVs[1].X, UVs[2].X,
            UVs[0].Y, UVs[1].Y, UVs[2].Y);

        sw.WriteLine("3 {0} {1} {2}", indices, Material, uvs);
    }
}