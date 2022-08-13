using System.Text;

namespace LeagueToolkit.IO.LightDat;

public class LightDatFile
{
    public LightDatFile(List<LightDatLight> lights)
    {
        Lights = lights;
    }

    public LightDatFile(string fileLocation)
        : this(File.OpenRead(fileLocation))
    {
    }

    public LightDatFile(Stream stream)
    {
        using (var sr = new StreamReader(stream))
        {
            while (!sr.EndOfStream)
            {
                Lights.Add(new LightDatLight(sr));
            }
        }
    }

    public List<LightDatLight> Lights { get; } = new();

    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    private void Write(Stream stream, bool leaveOpen = false)
    {
        using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen))
        {
            foreach (var light in Lights)
            {
                light.Write(sw);
            }
        }
    }
}