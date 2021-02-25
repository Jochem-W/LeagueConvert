using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeagueBulkConvert.WPF
{
    public class ListWriter : TextWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        private string line = "";

        private readonly IList<string> list;

        public override void Write(char value)
        {
            line += value;
            if (value == '\n')
                list.Add(line);
            line = "";
        }

        public override void Write(string value)
        {
            list.Add(value.Replace("\r", string.Empty).Replace("\n", string.Empty));
        }

        public ListWriter(IList<string> list)
        {
            this.list = list;
        }
    }
}
