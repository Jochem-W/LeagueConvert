using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeagueBulkConvert.WPF
{
    public class ListWriter : TextWriter
    {
        private readonly IList<string> _list;

        private string _line = "";

        public ListWriter(IList<string> list)
        {
            _list = list;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            _line += value;
            if (value == '\n')
                _list.Add(_line);
            _line = "";
        }

        public override void Write(string value)
        {
            _list.Add(value.Replace("\r", string.Empty).Replace("\n", string.Empty));
        }
    }
}