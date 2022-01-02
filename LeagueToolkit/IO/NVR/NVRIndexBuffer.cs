namespace LeagueToolkit.IO.NVR;

public class NVRIndexBuffer
{
    public NVRIndexBuffer(D3DFORMAT format)
    {
        Format = format;
    }

    public NVRIndexBuffer(BinaryReader br)
    {
        var length = br.ReadInt32();
        Format = (D3DFORMAT) br.ReadInt32();
        if (Format == D3DFORMAT.D3DFMT_INDEX16)
        {
            // 16-bit indices, all tested NVRs use this
            var indicesCount = length / 2;
            for (var i = 0; i < indicesCount; i++) Indices.Add(br.ReadUInt16());
        }
        else if (Format == D3DFORMAT.D3DFMT_INDEX32)
        {
            // 32-bit indices, never seen a NVR using this yet
            var indicesCount = length / 4;
            for (var i = 0; i < indicesCount; i++) Indices.Add(br.ReadInt32());
        }
        else
        {
            throw new UnsupportedD3DFORMATException(Format);
        }
    }

    public D3DFORMAT Format { get; }
    public List<int> Indices { get; } = new();
    public int CurrentMax { get; private set; } = -1;

    public void AddIndex(int indexToAdd)
    {
        Indices.Add(indexToAdd);
        if (CurrentMax < indexToAdd) CurrentMax = indexToAdd;
    }

    public void Write(BinaryWriter bw)
    {
        // Calculate length
        var indexLength = Format == D3DFORMAT.D3DFMT_INDEX16 ? 2 : 4;
        bw.Write(indexLength * Indices.Count);
        bw.Write((int) Format);
        foreach (var index in Indices)
            if (indexLength == 2)
                bw.Write((ushort) index);
            else
                bw.Write(index);
    }
}

public class UnsupportedD3DFORMATException : Exception
{
    public UnsupportedD3DFORMATException(D3DFORMAT actual) : base(
        string.Format("This D3DFORMAT ({0}) is not supported.", actual))
    {
    }
}