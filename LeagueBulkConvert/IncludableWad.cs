using System.IO;

namespace LeagueBulkConvert
{
    public class IncludableWad
    {
        private string path;

        public IncludableWad(string path)
        {
            FilePath = path;
        }

        public bool Included { get; set; }

        public string Name { get; private set; }

        public string FilePath
        {
            get => path;
            private set
            {
                path = value;
                Name = Path.GetFileName(path);
            }
        }
    }
}