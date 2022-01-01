using System.Collections.Generic;
using System.IO;
using System.Text;
using LeagueToolkit.Helpers.Exceptions;

namespace LeagueToolkit.IO.MapObjects;

/// <summary>
///     Represents a MapObjects.mob file
/// </summary>
public class MOBFile
{
    /// <summary>
    ///     Initializes an empty <see cref="MOBFile" />
    /// </summary>
    public MOBFile()
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="MOBFile" />
    /// </summary>
    /// <param name="objects">Objects of this <see cref="MOBFile" /></param>
    public MOBFile(List<MOBObject> objects)
    {
        Objects = objects;
    }

    /// <summary>
    ///     Initializes a new <see cref="MOBFile" /> from the specified location
    /// </summary>
    /// <param name="fileLocation">Location to read from</param>
    public MOBFile(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    /// <summary>
    ///     Initalizes a new <see cref="MOBFile" /> from the specified <see cref="Stream" />
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to read from</param>
    public MOBFile(Stream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            var magic = Encoding.ASCII.GetString(br.ReadBytes(4));
            if (magic != "OPAM") throw new InvalidFileSignatureException();

            var version = br.ReadUInt32();
            if (version != 2) throw new UnsupportedFileVersionException();

            var objectCount = br.ReadUInt32();
            br.ReadUInt32();

            for (var i = 0; i < objectCount; i++) Objects.Add(new MOBObject(br));
        }
    }

    /// <summary>
    ///     Objects of this <see cref="MOBFile" />
    /// </summary>
    public List<MOBObject> Objects { get; } = new();

    /// <summary>
    ///     Writes this <see cref="MOBFile" /> to the spcified location
    /// </summary>
    /// <param name="fileLocation">Location to write to</param>
    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    /// <summary>
    ///     Writes this <see cref="MOBFile" /> into a <see cref="Stream" />
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to write to</param>
    public void Write(Stream stream, bool leaveOpen = false)
    {
        using (var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
        {
            bw.Write(Encoding.ASCII.GetBytes("OPAM"));
            bw.Write((uint) 2);
            bw.Write(Objects.Count);
            bw.Write((uint) 0);

            foreach (var mobObject in Objects) mobObject.Write(bw);
        }
    }
}