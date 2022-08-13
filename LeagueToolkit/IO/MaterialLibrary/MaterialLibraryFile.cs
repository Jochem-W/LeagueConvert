using System.Text;

namespace LeagueToolkit.IO.MaterialLibrary;

public class MaterialLibraryFile
{
    public List<MaterialLibraryMaterial> Materials = new();

    public MaterialLibraryFile()
    {
    }

    public MaterialLibraryFile(string fileLocation)
        : this(File.OpenRead(fileLocation))
    {
    }

    public MaterialLibraryFile(Stream stream)
    {
        using (var sr = new StreamReader(stream))
        {
            while (!sr.EndOfStream)
            {
                if (sr.ReadLine() == "[MaterialBegin]")
                {
                    Materials.Add(new MaterialLibraryMaterial(sr));
                }
            }
        }
    }

    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    public void Write(Stream stream, bool leaveOpen = false)
    {
        using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen))
        {
            foreach (var material in Materials)
            {
                material.Write(sw);
                sw.WriteLine();
            }
        }
    }
}