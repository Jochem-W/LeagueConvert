using System.Text;

namespace LeagueToolkit.IO.WadFile;

public class WadEntry : IComparable<WadEntry>
{
    internal const int TOC_SIZE_V3 = 32;

    internal readonly Wad _wad;

    internal uint _dataOffset;
    internal bool _isDuplicated;

    internal WadEntry(Wad wad, ulong xxhash, int compressedSize, int uncompressedSize, WadEntryType entryType,
        WadEntryChecksumType checksumType, byte[] checksum, string fileRedirection, uint dataOffset)
    {
        _wad = wad;
        XXHash = xxhash;
        CompressedSize = compressedSize;
        UncompressedSize = uncompressedSize;
        Type = entryType;
        ChecksumType = checksumType;
        Checksum = checksum;
        FileRedirection = fileRedirection;
        _dataOffset = dataOffset;
    }

    internal WadEntry(Wad wad, BinaryReader br, byte major, byte minor)
    {
        _wad = wad;
        XXHash = br.ReadUInt64();
        _dataOffset = br.ReadUInt32();
        CompressedSize = br.ReadInt32();
        UncompressedSize = br.ReadInt32();
        Type = (WadEntryType) br.ReadByte();
        _isDuplicated = br.ReadBoolean();
        br.ReadUInt16(); // pad 
        if (major >= 2)
        {
            Checksum = br.ReadBytes(8);

            if (major == 3 && minor == 1) ChecksumType = WadEntryChecksumType.XXHash3;
            else ChecksumType = WadEntryChecksumType.SHA256;
        }

        if (Type == WadEntryType.FileRedirection)
        {
            var currentPosition = br.BaseStream.Position;
            br.BaseStream.Seek(_dataOffset, SeekOrigin.Begin);
            FileRedirection = Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt32()));
            br.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
        }
    }

    public ulong XXHash { get; }

    public int CompressedSize { get; internal set; }
    public int UncompressedSize { get; internal set; }

    public WadEntryType Type { get; }

    public WadEntryChecksumType ChecksumType { get; }
    public byte[] Checksum { get; internal set; }

    public string FileRedirection { get; }

    public int CompareTo(WadEntry other)
    {
        return XXHash.CompareTo(other.XXHash);
    }

    internal void Write(BinaryWriter bw, uint major)
    {
        bw.Write(XXHash);
        bw.Write(_dataOffset);
        bw.Write(CompressedSize);
        bw.Write(UncompressedSize);
        bw.Write((byte) Type);
        bw.Write(_isDuplicated);
        bw.Write((ushort) 0); // pad
        if (major >= 2) bw.Write(Checksum);
    }

    public WadEntryDataHandle GetDataHandle()
    {
        return new WadEntryDataHandle(this);
    }
}

public enum WadEntryType : byte
{
    /// <summary>
    ///     The Data of the <see cref="WadEntry" /> is uncompressed
    /// </summary>
    Uncompressed,

    /// <summary>
    ///     The Data of the <see cref="WadEntry" /> is compressed with GZip
    /// </summary>
    GZipCompressed,

    /// <summary>
    ///     The Data of this <see cref="WadEntry" /> is a file redirection
    /// </summary>
    FileRedirection,

    /// <summary>
    ///     The Data of this <see cref="WadEntry" /> is compressed with ZStandard
    /// </summary>
    ZStandardCompressed
}

public enum WadEntryChecksumType
{
    SHA256,
    XXHash3
}