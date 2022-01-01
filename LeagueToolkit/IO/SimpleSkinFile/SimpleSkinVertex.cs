using System.IO;
using System.Numerics;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.SimpleSkinFile;

public class SimpleSkinVertex
{
    public SimpleSkinVertex(Vector3 position, byte[] boneIndices, float[] weights, Vector3 normal, Vector2 uv)
    {
        Position = position;
        BoneIndices = boneIndices;
        Weights = weights;
        Normal = normal;
        UV = uv;
    }

    public SimpleSkinVertex(Vector3 position, byte[] boneIndices, float[] weights, Vector3 normal, Vector2 uv,
        Color color)
    {
        Position = position;
        BoneIndices = boneIndices;
        Weights = weights;
        Normal = normal;
        UV = uv;
        Color = color;
    }

    public SimpleSkinVertex(BinaryReader br, SimpleSkinVertexType vertexType)
    {
        Position = br.ReadVector3();
        BoneIndices = new[] {br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte()};
        Weights = new[] {br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle()};
        Normal = br.ReadVector3();
        UV = br.ReadVector2();

        if (vertexType == SimpleSkinVertexType.Color) Color = br.ReadColor(ColorFormat.RgbaU8);
    }

    public Vector3 Position { get; set; }
    public byte[] BoneIndices { get; set; }
    public float[] Weights { get; set; }
    public Vector3 Normal { get; set; }
    public Vector2 UV { get; set; }
    public Color? Color { get; set; }

    public void Write(BinaryWriter bw, SimpleSkinVertexType vertexType)
    {
        bw.WriteVector3(Position);

        for (var i = 0; i < 4; i++) bw.Write(BoneIndices[i]);

        for (var i = 0; i < 4; i++) bw.Write(Weights[i]);

        bw.WriteVector3(Normal);
        bw.WriteVector2(UV);

        if (vertexType == SimpleSkinVertexType.Color)
        {
            if (Color.HasValue)
                bw.WriteColor(Color.Value, ColorFormat.RgbaU8);
            else
                bw.WriteColor(new Color(0, 0, 0, 255), ColorFormat.RgbaU8);
        }
    }
}

public enum SimpleSkinVertexType : uint
{
    Basic,
    Color
}