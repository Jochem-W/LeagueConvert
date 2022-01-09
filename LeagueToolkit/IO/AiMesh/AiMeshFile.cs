using System.Text;
using LeagueToolkit.Helpers.Exceptions;

namespace LeagueToolkit.IO.AiMesh;

/// <summary>
///     Represents an AiMesh Navigation mesh
/// </summary>
public class AiMeshFile
{
    /// <summary>
    ///     Initializes a new <see cref="AiMeshFile" /> with cells
    /// </summary>
    public AiMeshFile(List<AiMeshCell> cells)
    {
        Cells = cells;
    }

    /// <summary>
    ///     Reads an <see cref="AiMeshFile" /> from the specified location
    /// </summary>
    /// <param name="fileLocation">The location to read from</param>
    public AiMeshFile(string fileLocation)
        : this(File.OpenRead(fileLocation))
    {
    }

    /// <summary>
    ///     Reads an <see cref="AiMeshFile" /> from the specified stream
    /// </summary>
    /// <param name="stream">Stream to read from</param>
    public AiMeshFile(Stream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            var magic = Encoding.ASCII.GetString(br.ReadBytes(8));
            if (magic != "r3d2aims") throw new InvalidFileSignatureException();

            var version = br.ReadUInt32();
            if (version != 2) throw new UnsupportedFileVersionException();

            var cellCount = br.ReadUInt32();
            var flags = br.ReadUInt32();
            var unknownFlagConstant = br.ReadUInt32(); // If set to [1] then Flags is [1]

            for (var i = 0; i < cellCount; i++) Cells.Add(new AiMeshCell(br));
        }
    }

    /// <summary>
    ///     A collection of <see cref="AiMeshCell" />
    /// </summary>
    public List<AiMeshCell> Cells { get; }

    /// <summary>
    ///     Writes this <see cref="AiMeshFile" /> to the specified location
    /// </summary>
    /// <param name="fileLocation">The location to write to</param>
    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    /// <summary>
    ///     Writes this <see cref="AiMeshFile" /> to the specified stream
    /// </summary>
    /// <param name="stream">Stream to write to</param>
    public void Write(Stream stream, bool leaveOpen = false)
    {
        using (var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
        {
            bw.Write(Encoding.ASCII.GetBytes("r3d2aims"));
            bw.Write((uint) 2);
            bw.Write(Cells.Count);
            bw.Write((uint) 0);
            bw.Write((uint) 0);

            foreach (var cell in Cells) cell.Write(bw);
        }
    }
}