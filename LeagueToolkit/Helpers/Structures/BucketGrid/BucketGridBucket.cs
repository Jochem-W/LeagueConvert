namespace LeagueToolkit.Helpers.Structures.BucketGrid;

public class BucketGridBucket
{
    public BucketGridBucket(BinaryReader br)
    {
        MaxStickOutX = br.ReadSingle();
        MaxStickOutZ = br.ReadSingle();
        StartIndex = br.ReadUInt32();
        BaseVertex = br.ReadUInt32();
        InsideFaceCount = br.ReadUInt16();
        StickingOutFaceCount = br.ReadUInt16();
    }

    public float MaxStickOutX { get; set; }
    public float MaxStickOutZ { get; set; }
    public uint StartIndex { get; set; }
    public uint BaseVertex { get; set; }
    public ushort InsideFaceCount { get; set; }
    public ushort StickingOutFaceCount { get; set; }

    public void Write(BinaryWriter bw)
    {
        bw.Write(MaxStickOutX);
        bw.Write(MaxStickOutZ);
        bw.Write(StartIndex);
        bw.Write(BaseVertex);
        bw.Write(InsideFaceCount);
        bw.Write(StickingOutFaceCount);
    }
}