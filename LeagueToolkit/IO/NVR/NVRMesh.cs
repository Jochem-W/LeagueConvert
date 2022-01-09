using System.Numerics;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.NVR;

public class NVRMesh
{
    //Used for writing
    public int MaterialIndex;

    public NVRMesh(BinaryReader br, NVRBuffers buffers, bool readOld)
    {
        QualityLevel = (NVRMeshQuality) br.ReadInt32();
        if (!readOld) Flag = br.ReadInt32();
        BoundingSphere = new R3DSphere(br);
        BoundingBox = new R3DBox(br);
        Material = buffers.Materials[br.ReadInt32()];
        IndexedPrimitives[0] = new NVRDrawIndexedPrimitive(br, buffers, this, true);
        IndexedPrimitives[1] = new NVRDrawIndexedPrimitive(br, buffers, this, false);
    }

    public NVRMesh(NVRMeshQuality meshQualityLevel, int flag, NVRMaterial material, List<NVRVertex> vertices,
        List<int> indices)
    {
        QualityLevel = meshQualityLevel;
        Flag = flag;
        Material = material;
        IndexedPrimitives[0] = new NVRDrawIndexedPrimitive(this, vertices, indices, true);
        IndexedPrimitives[1] = new NVRDrawIndexedPrimitive(this, vertices, indices, false);

        var min = new float[3] {vertices[0].Position.X, vertices[0].Position.Y, vertices[0].Position.Z};
        var max = new float[3] {vertices[0].Position.X, vertices[0].Position.Y, vertices[0].Position.Z};
        for (var i = 1; i < vertices.Count; i++)
        {
            var position = vertices[i].Position;
            if (position.X < min[0]) min[0] = position.X;
            if (position.Y < min[1]) min[1] = position.Y;
            if (position.Z < min[2]) min[2] = position.Z;
            if (position.X > max[0]) max[0] = position.X;
            if (position.Y > max[1]) max[1] = position.Y;
            if (position.Z > max[2]) max[2] = position.Z;
        }

        BoundingBox = new R3DBox(new Vector3(min[0], min[1], min[2]), new Vector3(max[0], max[1], max[2]));

        var radius = max[0] - min[0];
        if (max[1] - min[1] > radius) radius = max[1] - min[1];
        if (max[2] - min[2] > radius) radius = max[2] - min[2];
        BoundingSphere = new R3DSphere(new Vector3((min[0] + max[0]) / 2, (min[1] + max[1]) / 2, (min[2] + max[2]) / 2),
            radius / 2);
    }

    public NVRMeshQuality QualityLevel { get; set; }
    public int Flag { get; }
    public R3DSphere BoundingSphere { get; }
    public R3DBox BoundingBox { get; }
    public NVRMaterial Material { get; }
    public NVRDrawIndexedPrimitive[] IndexedPrimitives { get; } = new NVRDrawIndexedPrimitive[2];

    public void Write(BinaryWriter bw)
    {
        bw.Write((int) QualityLevel);
        bw.Write(Flag);
        BoundingSphere.Write(bw);
        BoundingBox.Write(bw);
        bw.Write(MaterialIndex);
        IndexedPrimitives[0].Write(bw);
        IndexedPrimitives[1].Write(bw);
    }

    private static Vector3 CalcNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        // Calculate two vectors from the three points
        var vector1 = new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        var vector2 = new Vector3(v2.X - v3.X, v2.Y - v3.Y, v2.Z - v3.Z);

        // Take the cross product of the two vectors to get
        // the normal vector which will be stored in out
        var norm = new Vector3(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);
        return norm;
    }
}

public enum NVRMeshQuality
{
    VERY_LOW = -100,

    // -1 should mean something
    LOW = 0,
    MEDIUM = 1,
    HIGH = 2,
    VERY_HIGH = 3
}