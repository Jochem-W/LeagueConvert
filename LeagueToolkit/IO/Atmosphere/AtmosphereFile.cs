using System.IO;
using System.Text;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.Atmosphere;

/// <summary>
///     Represents an Atmosphere file (Atmosphere.dat)
/// </summary>
public class AtmosphereFile
{
    /// <summary>
    ///     Initializes an empty <see cref="AtmosphereFile" />
    /// </summary>
    public AtmosphereFile()
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="AtmosphereFile" />
    /// </summary>
    /// <param name="sunColor">The color gradient of the Sun</param>
    /// <param name="skyColor">The color gradient of the Sky</param>
    public AtmosphereFile(TimeGradient sunColor, TimeGradient skyColor)
    {
        SunColor = sunColor;
        SkyColor = skyColor;
    }

    /// <summary>
    ///     Initialiazes a new <see cref="AtmosphereFile" /> from the specified location
    /// </summary>
    /// <param name="fileLocation">The location to read from</param>
    public AtmosphereFile(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="AtmosphereFile" /> from the specified <see cref="Stream" />
    /// </summary>
    /// <param name="stream"><The <see cref="Stream" /> to read from</param>
    public AtmosphereFile(Stream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            SunColor = new TimeGradient(br);
            SkyColor = new TimeGradient(br);
        }
    }

    /// <summary>
    ///     The color gradient of the Sun
    /// </summary>
    public TimeGradient SunColor { get; set; }

    /// <summary>
    ///     The color gradient of the Sky
    /// </summary>
    public TimeGradient SkyColor { get; set; }

    /// <summary>
    ///     Writes this <see cref="AtmosphereFile" /> to the specified location
    /// </summary>
    /// <param name="fileLocation">The location to write to</param>
    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    /// <summary>
    ///     Writes this <see cref="AtmosphereFile" /> into the specified <see cref="Stream" />
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to write to</param>
    public void Write(Stream stream, bool leaveOpen = false)
    {
        using (var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
        {
            SunColor.Write(bw);
            SkyColor.Write(bw);
        }
    }
}