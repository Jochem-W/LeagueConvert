using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LeagueToolkit.Helpers;
using LeagueToolkit.Helpers.Cryptography;
using XXHash3NET;

namespace LeagueToolkit.IO.WadFile;

public class WadEntryBuilder
{
    internal uint _dataOffset;
    internal bool _isGenericDataStream;

    public WadEntryBuilder(WadEntryChecksumType checksumType)
    {
        ChecksumType = checksumType;
    }

    public WadEntryBuilder(WadEntry entry)
    {
        ChecksumType = entry.ChecksumType;
        WithPathXXHash(entry.XXHash);

        switch (entry.Type)
        {
            case WadEntryType.Uncompressed:
                WithUncompressedDataStream(entry.GetDataHandle().GetDecompressedStream());
                break;
            case WadEntryType.GZipCompressed:
                WithGZipDataStream(entry.GetDataHandle().GetCompressedStream(), entry.CompressedSize,
                    entry.UncompressedSize);
                break;
            case WadEntryType.ZStandardCompressed:
                WithZstdDataStream(entry.GetDataHandle().GetCompressedStream(), entry.CompressedSize,
                    entry.UncompressedSize);
                break;
            case WadEntryType.FileRedirection:
                WithFileRedirection(entry.FileRedirection);
                break;
        }
    }

    public WadEntryType EntryType { get; private set; }
    public ulong PathXXHash { get; private set; }

    public Stream DataStream { get; internal set; }
    public int CompressedSize { get; internal set; }
    public int UncompressedSize { get; internal set; }

    public string FileRedirection { get; private set; }

    public WadEntryChecksumType ChecksumType { get; internal set; }
    public byte[] Checksum { get; internal set; }

    public WadEntryBuilder WithPath(string path)
    {
        return WithPathXXHash(XXHash.XXH64(Encoding.UTF8.GetBytes(path.ToLower())));
    }

    public WadEntryBuilder WithPathXXHash(ulong hash)
    {
        PathXXHash = hash;

        return this;
    }

    public WadEntryBuilder WithZstdDataStream(Stream stream, int compressedSize, int uncompressedSize)
    {
        EntryType = WadEntryType.ZStandardCompressed;
        DataStream = stream;
        CompressedSize = compressedSize;
        UncompressedSize = uncompressedSize;

        ComputeChecksum();

        return this;
    }

    public WadEntryBuilder WithGZipDataStream(Stream stream, int compressedSize, int uncompressedSize)
    {
        EntryType = WadEntryType.GZipCompressed;
        DataStream = stream;
        CompressedSize = compressedSize;
        UncompressedSize = uncompressedSize;

        ComputeChecksum();

        return this;
    }

    public WadEntryBuilder WithUncompressedDataStream(Stream stream)
    {
        EntryType = WadEntryType.Uncompressed;
        DataStream = stream;
        CompressedSize = UncompressedSize = (int) stream.Length;

        ComputeChecksum();

        return this;
    }

    public WadEntryBuilder WithFileDataStream(string fileLocation)
    {
        return WithFileDataStream(File.OpenRead(fileLocation));
    }

    public WadEntryBuilder WithFileDataStream(FileStream stream)
    {
        var filePath = stream.Name;

        return WithGenericDataStream(filePath, stream);
    }

    public WadEntryBuilder WithGenericDataStream(string path, Stream stream)
    {
        EntryType = Utilities.GetExtensionWadCompressionType(Path.GetExtension(path));
        DataStream = stream;
        _isGenericDataStream = true;
        Checksum = new byte[8];

        return this;
    }

    public WadEntryBuilder WithFileRedirection(string fileRedirection)
    {
        EntryType = WadEntryType.FileRedirection;
        FileRedirection = fileRedirection;
        CompressedSize = UncompressedSize = fileRedirection.Length + 4;
        Checksum = new byte[8];

        return this;
    }

    internal void ComputeChecksum()
    {
        if (ChecksumType == WadEntryChecksumType.SHA256)
        {
            using (var sha = SHA256.Create())
            {
                DataStream.Seek(0, SeekOrigin.Begin);

                Checksum = sha.ComputeHash(DataStream).Take(8).ToArray();
            }
        }
        else if (ChecksumType == WadEntryChecksumType.XXHash3)
        {
            var data = new byte[DataStream.Length];

            DataStream.Seek(0, SeekOrigin.Begin);
            DataStream.Read(data, 0, data.Length);

            Checksum = BitConverter.GetBytes(XXHash3.Hash64(data));
        }
    }
}