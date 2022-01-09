using System.Globalization;
using System.Numerics;

namespace LeagueToolkit.IO.MapParticles;

/// <summary>
///     Represents a particle inside a <see cref="MapParticlesFile" />
/// </summary>
public class MapParticlesParticle
{
    /// <summary>
    ///     Initializes a blank <see cref="MapParticlesParticle" />
    /// </summary>
    public MapParticlesParticle()
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="MapParticlesParticle" />
    /// </summary>
    /// <param name="name">Name of the particle</param>
    /// <param name="position">Position of the particle</param>
    /// <param name="quality">World Quality of the particle</param>
    /// <param name="rotation">Rotation of the particle</param>
    /// <param name="tags">Special Tags of the particle</param>
    public MapParticlesParticle(string name, Vector3 position, int quality, Vector3 rotation, List<string> tags)
    {
        Name = name;
        Position = position;
        Quality = quality;
        Rotation = rotation;
        Tags = tags;
    }

    /// <summary>
    ///     Initializes a new <see cref="MapParticlesParticle" /> from a <see cref="StreamReader" />
    /// </summary>
    /// <param name="sr">The <see cref="StreamReader" /> to read from</param>
    public MapParticlesParticle(StreamReader sr)
    {
        var input = sr.ReadLine().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
        Name = input[0];

        Position = new Vector3(float.Parse(input[1], CultureInfo.InvariantCulture),
            float.Parse(input[2], CultureInfo.InvariantCulture),
            float.Parse(input[3], CultureInfo.InvariantCulture));

        Quality = int.Parse(input[4]);

        Rotation = new Vector3(float.Parse(input[5], CultureInfo.InvariantCulture),
            float.Parse(input[6], CultureInfo.InvariantCulture),
            float.Parse(input[7], CultureInfo.InvariantCulture));

        Tags.AddRange(input.ToList().GetRange(8, input.Length - 8));
    }

    /// <summary>
    ///     Name of this <see cref="MapParticlesParticle" />
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Position of this <see cref="MapParticlesParticle" />
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    ///     World Quality of this <see cref="MapParticlesParticle" />
    /// </summary>
    public int Quality { get; set; }

    /// <summary>
    ///     Roatation of this <see cref="MapParticlesParticle" />
    /// </summary>
    public Vector3 Rotation { get; set; }

    /// <summary>
    ///     Special Tags of this <see cref="MapParticlesParticle" />
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    ///     Writes this <see cref="MapParticlesParticle" /> into a <see cref="StreamWriter" />
    /// </summary>
    /// <param name="sw">The <see cref="StreamWriter" /> to write to</param>
    public void Write(StreamWriter sw)
    {
        var write = string.Format("{0} {1} {2} {3} {4} {5} {6} {7}",
            Name,
            Position.X,
            Position.Y,
            Position.Z,
            Quality,
            Rotation.X,
            Rotation.Y,
            Rotation.Z
        );

        foreach (var tag in Tags) write += " " + tag;

        sw.WriteLine(write);
    }
}