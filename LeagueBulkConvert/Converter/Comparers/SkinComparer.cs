using System.Collections.Generic;

namespace LeagueBulkConvert.Converter.Comparers
{
    class SkinComparer : IComparer<string>
    {
        int IComparer<string>.Compare(string x, string y)
        {
            if (x == null)
            {
                if (y == null)
                    return 0;
                else
                    return 1;
            }
            else
            {
                if (y == null)
                    return -1;
                else
                {
                    if (y.Contains("Original "))
                        return 1;
                    else if (x.Contains("Original "))
                        return -1;
                    else
                        return x.CompareTo(y);
                }
            }
        }
    }
}
