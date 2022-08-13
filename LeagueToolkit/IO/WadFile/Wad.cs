using System.Collections.ObjectModel;
using System.Text;
using LeagueToolkit.Helpers.Exceptions;

namespace LeagueToolkit.IO.WadFile;

public class Wad : IDisposable
{
    internal const int HEADER_SIZE_V3 = 272;
    private readonly Dictionary<ulong, WadEntry> _entries = new();

    private readonly bool _leaveOpen;
    private bool _isDisposed;

    internal Stream _stream;

    internal Wad()
    {
        Entries = new ReadOnlyDictionary<ulong, WadEntry>(_entries);
    }

    private Wad(Stream stream) : this()
    {
        _stream = stream;
    }

    internal Wad(Stream stream, bool leaveOpen) : this(stream)
    {
        _leaveOpen = leaveOpen;

        Read(_stream, leaveOpen);
    }

    public byte[] Signature { get; private set; }

    public ReadOnlyDictionary<ulong, WadEntry> Entries { get; }

    public void Dispose()
    {
        if (_isDisposed is false)
        {
            if (_leaveOpen is false)
            {
                _stream?.Close();
            }

            _isDisposed = true;
        }
    }

    public static Wad Mount(string fileLocation, bool leaveOpen)
    {
        return Mount(File.OpenRead(fileLocation), leaveOpen);
    }

    public static Wad Mount(Stream stream, bool leaveOpen)
    {
        return new Wad(stream, leaveOpen);
    }

    private void Read(Stream stream, bool leaveOpen)
    {
        using (var br = new BinaryReader(stream, Encoding.UTF8, leaveOpen))
        {
            var magic = Encoding.ASCII.GetString(br.ReadBytes(2));
            if (magic != "RW")
            {
                throw new InvalidFileSignatureException();
            }

            var major = br.ReadByte();
            var minor = br.ReadByte();
            if (major > 3)
            {
                throw new UnsupportedFileVersionException();
            }

            ulong dataChecksum = 0; // probably not "dataChecksum"

            if (major == 2)
            {
                var ecdsaLength = br.ReadByte();
                Signature = br.ReadBytes(ecdsaLength);
                br.ReadBytes(83 - ecdsaLength);

                dataChecksum = br.ReadUInt64();
            }
            else if (major == 3)
            {
                Signature = br.ReadBytes(256);
                dataChecksum = br.ReadUInt64();
            }

            if (major == 1 || major == 2)
            {
                var tocStartOffset = br.ReadUInt16();
                var tocFileEntrySize = br.ReadUInt16();
            }

            var fileCount = br.ReadUInt32();
            for (var i = 0; i < fileCount; i++)
            {
                var entry = new WadEntry(this, br, major, minor);

                if (_entries.ContainsKey(entry.XXHash))
                {
                    throw new InvalidOperationException(
                        "Tried to read a Wad Entry with the same path hash as an already existing entry: " +
                        entry.XXHash);
                }

                _entries.Add(entry.XXHash, entry);
            }
        }
    }

    internal void Write(Stream stream, bool leaveOpen)
    {
        using (var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
        {
            bw.Write(Encoding.ASCII.GetBytes("RW"));
            bw.Write((byte)3); // major
            bw.Write((byte)1); // minor

            // Writing signature
            bw.Write(new byte[256]);

            bw.Write((long)0);

            var tocSize = 32;
            var tocOffset = stream.Position + 4;

            bw.Write(Entries.Count);

            stream.Seek(tocOffset + tocSize * Entries.Count, SeekOrigin.Begin);

            var entryKeys = _entries.Keys.ToList();
            entryKeys.Sort();

            // Write TOC
            stream.Seek(tocOffset, SeekOrigin.Begin);
            foreach (var entryKey in entryKeys)
            {
                _entries[entryKey].Write(bw, 3);
            }
        }
    }

    internal void AddEntry(WadEntry entry)
    {
        if (_entries.ContainsKey(entry.XXHash))
        {
            throw new InvalidOperationException(
                "Tried to add an entry with an already existing XXHash: " + entry.XXHash);
        }

        _entries.Add(entry.XXHash, entry);
    }
}