using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using LeagueToolkit.Helpers.Extensions;

namespace LeagueToolkit.Helpers.Structures.BucketGrid;

public class BucketGrid
{
    public BucketGrid(BinaryReader br)
    {
        MinX = br.ReadSingle();
        MinZ = br.ReadSingle();
        MaxX = br.ReadSingle();
        MaxZ = br.ReadSingle();
        MaxOutStickX = br.ReadSingle();
        MaxOutStickZ = br.ReadSingle();
        BucketSizeX = br.ReadSingle();
        BucketSizeZ = br.ReadSingle();

        var bucketsPerSide = br.ReadUInt16();
        var unknown = br.ReadUInt16();
        var vertexCount = br.ReadUInt32();
        var indexCount = br.ReadUInt32();

        for (var i = 0; i < vertexCount; i++) Vertices.Add(br.ReadVector3());

        for (var i = 0; i < indexCount; i++) Indices.Add(br.ReadUInt16());

        Buckets = new BucketGridBucket[bucketsPerSide, bucketsPerSide];
        for (var i = 0; i < bucketsPerSide; i++)
        for (var j = 0; j < bucketsPerSide; j++)
            Buckets[i, j] = new BucketGridBucket(br);
    }

    public float MinX { get; set; }
    public float MinZ { get; set; }
    public float MaxX { get; set; }
    public float MaxZ { get; set; }
    public float MaxOutStickX { get; set; }
    public float MaxOutStickZ { get; set; }
    public float BucketSizeX { get; set; }
    public float BucketSizeZ { get; set; }
    public List<Vector3> Vertices { get; set; } = new();
    public List<ushort> Indices { get; set; } = new();
    public BucketGridBucket[,] Buckets { get; set; }

    public void Write(BinaryWriter bw)
    {
        bw.Write(MinX);
        bw.Write(MinZ);

        bw.Write(MaxX);
        bw.Write(MaxZ);

        bw.Write(MaxOutStickX);
        bw.Write(MaxOutStickZ);

        bw.Write(BucketSizeX);
        bw.Write(BucketSizeZ);

        var bucketsPerSide = (uint) Math.Sqrt(Buckets.Length);
        bw.Write(bucketsPerSide);
        bw.Write(Vertices.Count);
        bw.Write(Indices.Count);

        foreach (var vertex in Vertices) bw.WriteVector3(vertex);
        foreach (var index in Indices) bw.Write(index);
        for (var i = 0; i < bucketsPerSide; i++)
        for (var j = 0; j < bucketsPerSide; j++)
            Buckets[i, j].Write(bw);
    }
}