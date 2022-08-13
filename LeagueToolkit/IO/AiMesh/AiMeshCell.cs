using System.Numerics;
using LeagueToolkit.Helpers.Extensions;

namespace LeagueToolkit.IO.AiMesh;

/// <summary>
///     Represents a cell inside of an <see cref="AiMeshFile" />
/// </summary>
public class AiMeshCell
{
    /// <summary>
    ///     Reads an <see cref="AiMeshCell" /> from a <see cref="BinaryReader" />
    /// </summary>
    /// <param name="br">The <see cref="BinaryReader" /> to read from</param>
    public AiMeshCell(BinaryReader br)
    {
        Vertices = new[] { br.ReadVector3(), br.ReadVector3(), br.ReadVector3() };
        Links = new[] { br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16() };
    }

    /// <summary>
    ///     Vertices of this <see cref="AiMeshCell" />
    /// </summary>
    public Vector3[] Vertices { get; set; }

    /// <summary>
    ///     Links to other <see cref="AiMeshCell" />
    /// </summary>
    /// <remarks>A link can be 0xFF which means it doesnt link to anything</remarks>
    public ushort[] Links { get; set; }

    /// <summary>
    ///     Writes this <see cref="AiMeshCell" /> into a <see cref="BinaryWriter" />
    /// </summary>
    /// <param name="bw">The <see cref="BinaryWriter" /> to write to</param>
    public void Write(BinaryWriter bw)
    {
        for (var i = 0; i < 3; i++)
        {
            bw.WriteVector3(Vertices[i]);
        }

        for (var i = 0; i < 3; i++)
        {
            bw.Write(Links[i]);
        }
    }
}