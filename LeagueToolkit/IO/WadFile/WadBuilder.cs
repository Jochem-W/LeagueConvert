using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Text;
using ImpromptuNinjas.ZStd;

namespace LeagueToolkit.IO.WadFile;

public class WadBuilder : IDisposable
{
    private readonly Dictionary<ulong, WadEntryBuilder> _entries = new();

    private bool _disposedValue;

    public WadBuilder()
    {
        Entries = new ReadOnlyDictionary<ulong, WadEntryBuilder>(_entries);
    }

    public WadBuilder(Wad wad) : this()
    {
        foreach (var entry in wad.Entries) WithEntry(new WadEntryBuilder(entry.Value));
    }

    public ReadOnlyDictionary<ulong, WadEntryBuilder> Entries { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing)
            foreach (var entry in _entries)
                entry.Value.DataStream.Dispose();

        _disposedValue = true;
    }

    public WadBuilder WithEntry(WadEntryBuilder entryBuilder)
    {
        if (_entries.ContainsKey(entryBuilder.PathXXHash))
            throw new InvalidOperationException("An entry with the same XXHash has already been added: " +
                                                entryBuilder.PathXXHash);
        _entries.Add(entryBuilder.PathXXHash, entryBuilder);

        return this;
    }

    public void Build(string fileLocation)
    {
        Build(File.OpenWrite(fileLocation), false);
    }

    public void Build(Stream stream, bool leaveOpen)
    {
        using Wad wad = new();

        var headerStartOffset = stream.Position;

        // Seek after Header and TOC
        stream.Seek(Wad.HEADER_SIZE_V3 + _entries.Count * WadEntry.TOC_SIZE_V3, SeekOrigin.Current);

        // Write data streams into the stream and save offsets
        foreach (var entryBuilder in _entries.Values) WriteEntryData(stream, entryBuilder);

        foreach (var entryBuilder in _entries.Values)
            wad.AddEntry(new WadEntry(
                wad,
                entryBuilder.PathXXHash,
                entryBuilder.CompressedSize,
                entryBuilder.UncompressedSize,
                entryBuilder.EntryType,
                entryBuilder.ChecksumType,
                entryBuilder.Checksum,
                entryBuilder.FileRedirection,
                entryBuilder._dataOffset)
            );

        // Seek to start
        stream.Seek(headerStartOffset, SeekOrigin.Begin);

        wad.Write(stream, leaveOpen);
    }

    private void WriteEntryData(Stream wadStream, WadEntryBuilder entryBuilder)
    {
        // If we're writing a File Stream then we need to compress it first
        if (entryBuilder._isGenericDataStream)
        {
            var uncompressedSize = (int) entryBuilder.DataStream.Length;
            var compressedStream = new MemoryStream();
            if (entryBuilder.EntryType == WadEntryType.GZipCompressed)
            {
                using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress, true))
                {
                    entryBuilder.DataStream.CopyTo(gzipStream);
                }

                entryBuilder.DataStream = compressedStream;
                entryBuilder.UncompressedSize = uncompressedSize;
                entryBuilder.CompressedSize = (int) compressedStream.Length;
            }
            else if (entryBuilder.EntryType == WadEntryType.ZStandardCompressed)
            {
                using (var compressStream = new ZStdCompressStream(compressedStream))
                {
                    entryBuilder.DataStream.CopyTo(compressStream);
                    compressStream.Flush();
                }

                entryBuilder.DataStream = compressedStream;
                entryBuilder.UncompressedSize = uncompressedSize;
                entryBuilder.CompressedSize = (int) compressedStream.Length;
            }
            else if (entryBuilder.EntryType == WadEntryType.Uncompressed)
            {
                entryBuilder.CompressedSize = entryBuilder.UncompressedSize = (int) entryBuilder.DataStream.Length;
            }
            else
            {
                throw new InvalidOperationException("Cannot have a File Redirection entry with a data stream");
            }

            entryBuilder.ComputeChecksum();
        }

        entryBuilder._dataOffset = (uint) wadStream.Position;

        // Write data
        if (entryBuilder.EntryType == WadEntryType.FileRedirection)
        {
            wadStream.Write(BitConverter.GetBytes(entryBuilder.FileRedirection.Length));
            wadStream.Write(Encoding.UTF8.GetBytes(entryBuilder.FileRedirection));
        }
        else
        {
            var data = new byte[entryBuilder.DataStream.Length];

            entryBuilder.DataStream.Seek(0, SeekOrigin.Begin);
            entryBuilder.DataStream.Read(data, 0, data.Length);

            wadStream.Write(data, 0, data.Length);
        }
    }

    public void RemoveEntry(ulong xxhash)
    {
        _entries.Remove(xxhash);
    }
}