using System.IO;

namespace LeagueBulkConvert;

public class IncludableWad
{
    private readonly string _path;

    public IncludableWad(string path)
    {
        FilePath = path;
    }

    public bool Included { get; set; }

    public string Name { get; private init; }

    public string FilePath
    {
        get => _path;
        private init
        {
            _path = value;
            Name = Path.GetFileName(_path);
        }
    }
}