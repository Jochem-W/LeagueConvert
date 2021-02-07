using System.IO;

namespace LeagueBulkConvert
{
    public class IncludableWad
    {
        public bool Included { get; set; }

        public string Name { get; private set; }

        private string path;
        public string FilePath
        {
            get => path;
            private set
            {
                path = value;
                Name = Path.GetFileName(path);
            }
        }

        public IncludableWad(string path) => FilePath = path;
    }
}
