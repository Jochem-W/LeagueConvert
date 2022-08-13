using System.Diagnostics;
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
        Uv = uv;
    }

    public SimpleSkinVertex(Vector3 position, byte[] boneIndices, float[] weights, Vector3 normal, Vector2 uv,
        Color color)
    {
        Position = position;
        BoneIndices = boneIndices;
        Weights = weights;
        Normal = normal;
        Uv = uv;
        Color = color;
    }

    public SimpleSkinVertex(BinaryReader br, SimpleSkinVertexType vertexType)
    {
        Position = br.ReadVector3();
        BoneIndices = new[] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
        Weights = new[] { br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle() };
        Normal = br.ReadVector3();

        var uvX = br.ReadSingle();
        var uvY = br.ReadSingle();
        if (float.IsPositiveInfinity(uvX))
        {
            uvX = float.MaxValue;
        }
        else if (float.IsNegativeInfinity(uvX))
        {
            uvX = float.MinValue;
        }

        if (float.IsPositiveInfinity(uvY))
        {
            uvY = float.MaxValue;
        }
        else if (float.IsNegativeInfinity(uvY))
        {
            uvY = float.MinValue;
        }

        Uv = new Vector2(uvX, uvY);

        if (vertexType is SimpleSkinVertexType.Color or SimpleSkinVertexType.ColorAndTangent)
        {
            Color = br.ReadColor(ColorFormat.RgbaU8);
        }

        if (vertexType == SimpleSkinVertexType.ColorAndTangent)
        {
            Tangent = br.ReadVector4();
        }

        Debug.Assert(Tangent == null || Math.Abs(Math.Abs(Tangent.Value.W) - 1f) < 0.01f);

        CalculateNormal();
    }

    public Vector3 Position { get; set; }
    public byte[] BoneIndices { get; set; }
    public float[] Weights { get; set; }
    public Vector3 Normal { get; set; }
    public Vector2 Uv { get; set; }
    public Color? Color { get; set; }
    public Vector4? Tangent { get; set; }

    public void Write(BinaryWriter bw, SimpleSkinVertexType vertexType)
    {
        bw.WriteVector3(Position);

        for (var i = 0; i < 4; i++)
        {
            bw.Write(BoneIndices[i]);
        }

        for (var i = 0; i < 4; i++)
        {
            bw.Write(Weights[i]);
        }

        bw.WriteVector3(Normal);
        bw.WriteVector2(Uv);

        if (vertexType == SimpleSkinVertexType.Color)
        {
            bw.WriteColor(Color ?? new Color(0, 0, 0, 255), ColorFormat.RgbaU8);
        }
    }

    private void CalculateNormal()
    {
        var originalNormal = Normal;
        Normal = Vector3.Normalize(Normal);
        if (!float.IsNaN(Normal.Length()))
        {
            return;
        }

        // Previous attempts at creating a normal were incorrect. Doing it the proper way might incur a big performance
        // hit, but is worth looking into. For now, fall back to the position vector.

        // Extrapolate 0 --> 1 and -0 --> -1 from the original normal, then normalise
        if (!float.IsNaN(originalNormal.Length()))
        {
            var x = BitConverter.SingleToInt32Bits(originalNormal.X) < 0 ? -1 : 1;
            var y = BitConverter.SingleToInt32Bits(originalNormal.Y) < 0 ? -1 : 1;
            var z = BitConverter.SingleToInt32Bits(originalNormal.Z) < 0 ? -1 : 1;
            Normal = Vector3.Normalize(new Vector3(x, y, z));
            if (!float.IsNaN(Normal.Length()))
            {
                return;
            }
        }

        Normal = Vector3.Normalize(Position);

        Debug.Assert(!float.IsNaN(Normal.Length()));
    }
}