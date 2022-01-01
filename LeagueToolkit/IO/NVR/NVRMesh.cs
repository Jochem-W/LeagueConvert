using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using LeagueToolkit.Helpers.Structures;
using LeagueToolkit.IO.OBJ;

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

    public static Tuple<List<NVRVertex>, List<int>> GetGeometryFromOBJ(OBJFile objFile)
    {
        var vertices = new List<NVRVertex>();
        var indices = new List<int>();

        // We first add all the vertices in a list.
        var objVertices = new List<NVRVertex>();
        foreach (var position in objFile.Vertices) objVertices.Add(new NVRVertex8(position));
        foreach (var face in objFile.Faces)
            for (var i = 0; i < 3; i++)
            {
                var position = (NVRVertex8) objVertices[(int) face.VertexIndices[i]];
                var UV = new Vector2(0, 0);
                if (objFile.UVs.Count > 0) UV = objFile.UVs[(int) face.UVIndices[i]];
                var normal = new Vector3(0, 0, 0);
                if (objFile.Normals.Count > 0) normal = objFile.Normals[(int) face.NormalIndices[i]];

                if (position.UV != null && position.Normal != null &&
                    (!position.UV.Equals(UV) || !position.Normal.Equals(normal)))
                    // Needs to replicate
                    position = new NVRVertex8(position.Position);
                position.UV = UV;
                position.Normal = normal;
                position.DiffuseColor = new Color(0, 0, 0, 255);
                position.EmissiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

                var vertexIndex = vertices.IndexOf(position);
                if (vertexIndex == -1)
                {
                    vertexIndex = vertices.Count;
                    vertices.Add(position);
                }

                indices.Add(vertexIndex);
            }

        //for (int i = 0; i < indices.Count; i += 3)
        //{
        //    NVRVertex8 v1 = (NVRVertex8)vertices[indices[i]];
        //    NVRVertex8 v2 = (NVRVertex8)vertices[indices[i + 1]];
        //    NVRVertex8 v3 = (NVRVertex8)vertices[indices[i + 2]];
        //    Vector3 faceNormal = CalcNormal(v1.Position, v2.Position, v3.Position);
        //    v1.Normal = v1.Normal + faceNormal;
        //    v2.Normal = v2.Normal + faceNormal;
        //    v3.Normal = v3.Normal + faceNormal;
        //}
        //foreach (NVRVertex8 vert in vertices)
        //{
        //    float length = (float)Math.Sqrt(Math.Pow(vert.Normal.X, 2) + Math.Pow(vert.Normal.Y, 2) + Math.Pow(vert.Normal.Z, 2));
        //    vert.Normal.X = vert.Normal.X / length;
        //    vert.Normal.Y = vert.Normal.Y / length;
        //    vert.Normal.Z = vert.Normal.Z / length;
        //}

        return new Tuple<List<NVRVertex>, List<int>>(vertices, indices);
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