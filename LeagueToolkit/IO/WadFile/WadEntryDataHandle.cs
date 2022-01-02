using System.IO.Compression;
using ZstdSharp;

namespace LeagueToolkit.IO.WadFile;

public struct WadEntryDataHandle
{
    private readonly WadEntry _entry;

    internal WadEntryDataHandle(WadEntry entry)
    {
        _entry = entry;
    }

    public Stream GetCompressedStream()
    {
        var wadStream = _entry._wad._stream;

        // Seek to entry data
        wadStream.Seek(_entry._dataOffset, SeekOrigin.Begin);

        // Read compressed data to a buffer
        var compressedData = new byte[_entry.CompressedSize];
        wadStream.Read(compressedData, 0, _entry.CompressedSize);

        switch (_entry.Type)
        {
            case WadEntryType.GZipCompressed:
            case WadEntryType.ZStandardCompressed:
            case WadEntryType.Uncompressed:
            {
                return new MemoryStream(compressedData);
            }
            case WadEntryType.FileRedirection:
            {
                throw new InvalidOperationException("Cannot open a handle to a File Redirection");
            }
            default:
            {
                throw new InvalidOperationException("Invalid Wad Entry type: " + _entry.Type);
            }
        }
    }

    public Stream GetDecompressedStream()
    {
        var wadStream = _entry._wad._stream;

        // Seek to entry data
        wadStream.Seek(_entry._dataOffset, SeekOrigin.Begin);

        // Read compressed data to a buffer
        var compressedData = new byte[_entry.CompressedSize];
        wadStream.Read(compressedData, 0, _entry.CompressedSize);

        switch (_entry.Type)
        {
            case WadEntryType.GZipCompressed:
            {
                var uncompressedStream = new MemoryStream(_entry.UncompressedSize);
                using var compressedStream = new MemoryStream(compressedData);
                using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);

                gzipStream.CopyTo(uncompressedStream);

                return uncompressedStream;
            }
            case WadEntryType.ZStandardCompressed:
            {
                var decompressedData = Zstd.Decompress(compressedData, _entry.UncompressedSize);

                return new MemoryStream(decompressedData);
            }
            case WadEntryType.Uncompressed:
            {
                return new MemoryStream(compressedData);
            }
            case WadEntryType.FileRedirection:
            {
                throw new InvalidOperationException("Cannot open a handle to a File Redirection");
            }
            default:
            {
                throw new InvalidOperationException("Invalid Wad Entry type: " + _entry.Type);
            }
        }
    }
}