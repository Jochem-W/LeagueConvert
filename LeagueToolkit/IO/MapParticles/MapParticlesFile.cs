using System.Text;

namespace LeagueToolkit.IO.MapParticles;

/// <summary>
///     Represents a Particles.dat file which holds data about the particles of a map
/// </summary>
public class MapParticlesFile
{
    /// <summary>
    ///     Initializes a blank <see cref="MapParticlesFile" />
    /// </summary>
    public MapParticlesFile()
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="MapParticlesFile" />
    /// </summary>
    /// <param name="particles">The particles of the <see cref="MapParticlesFile" /></param>
    public MapParticlesFile(List<MapParticlesParticle> particles)
    {
        Particles = particles;
    }

    /// <summary>
    ///     Initializes a new <see cref="MapParticlesFile" /> from the specified location
    /// </summary>
    /// <param name="fileLocation">The location to read from</param>
    public MapParticlesFile(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="MapParticlesFile" /> from a <see cref="Stream" />
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to read from</param>
    public MapParticlesFile(Stream stream)
    {
        Particles = new List<MapParticlesParticle>();
        using (var sr = new StreamReader(stream))
        {
            while (!sr.EndOfStream)
            {
                Particles.Add(new MapParticlesParticle(sr));
            }
        }
    }

    /// <summary>
    ///     Particles of this <see cref="MapParticlesFile" />
    /// </summary>
    public List<MapParticlesParticle> Particles { get; }

    /// <summary>
    ///     Writes this <see cref="MapParticlesFile" /> to the specified location
    /// </summary>
    /// <param name="fileLocation">The location to write to</param>
    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    /// <summary>
    ///     Writes this <see cref="MapParticlesFile" /> into the specified <see cref="Stream" />
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to write to</param>
    public void Write(Stream stream, bool leaveOpen = false)
    {
        using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen))
        {
            foreach (var particle in Particles)
            {
                particle.Write(sw);
            }
        }
    }
}