using System;
using System.IO;

namespace LeagueToolkit.IO.WorldDescription;

/// <summary>
///     Represents an object inside a <see cref="WorldDescriptionFile" />
/// </summary>
public class WorldDescriptionObject
{
    /// <summary>
    ///     Initializes a new <see cref="WorldDescriptionObject" />
    /// </summary>
    public WorldDescriptionObject()
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="WorldDescriptionObject" />
    /// </summary>
    /// <param name="name">The Name of this <see cref="WorldDescriptionObject" /></param>
    /// <param name="worldQuality">The World Quality of this <see cref="WorldDescriptionObject" /></param>
    public WorldDescriptionObject(string name, int worldQuality)
    {
        Name = name;
        WorldQuality = worldQuality;
    }

    /// <summary>
    ///     Initializes a new <see cref="WorldDescriptionObject" /> from a <see cref="StreamReader" />
    /// </summary>
    /// <param name="sr">The <see cref="StreamReader" /> to read from</param>
    public WorldDescriptionObject(StreamReader sr)
    {
        var line = sr.ReadLine().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

        Name = line[0];
        WorldQuality = int.Parse(line[1]);
    }

    /// <summary>
    ///     Name of this <see cref="WorldDescriptionObject" />
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     World Quality of this <see cref="WorldDescriptionObject" />
    /// </summary>
    public int WorldQuality { get; set; }

    /// <summary>
    ///     Writes this <see cref="WorldDescriptionObject" /> into a <see cref="StreamWriter" />
    /// </summary>
    /// <param name="sw">The <see cref="StreamWriter" /> to write to</param>
    public void Write(StreamWriter sw)
    {
        sw.WriteLine("{0} {1}", Name, WorldQuality);
    }
}