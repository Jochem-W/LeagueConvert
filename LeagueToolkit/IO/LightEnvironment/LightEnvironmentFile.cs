using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeagueToolkit.IO.LightEnvironment;

public class LightEnvironmentFile
{
    public LightEnvironmentFile(List<LightEnvironmentLight> lights)
    {
        Lights = lights;
    }

    public LightEnvironmentFile(string fileLocation)
        : this(File.OpenRead(fileLocation))
    {
    }

    public LightEnvironmentFile(Stream stream)
    {
        using (var sr = new StreamReader(stream))
        {
            var lightVersion = sr.ReadLine();
            while (!sr.EndOfStream) Lights.Add(new LightEnvironmentLight(sr));
        }
    }

    public List<LightEnvironmentLight> Lights { get; } = new();

    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    private void Write(Stream stream, bool leaveOpen = false)
    {
        using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen))
        {
            sw.WriteLine("3");

            foreach (var light in Lights) light.Write(sw);
        }
    }
}