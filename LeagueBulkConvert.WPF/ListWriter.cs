using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeagueBulkConvert.WPF
{
    public class ListWriter : TextWriter
    {
        private readonly IList<string> list;

        private string line = "";

        public ListWriter(IList<string> list)
        {
            this.list = list;
        }

        public override Encoding Encoding => Encoding.UTF8;

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
    }
}