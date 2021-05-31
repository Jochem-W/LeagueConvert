using System.Collections.Generic;

namespace SimpleGltf.Helpers
{
    internal class StrideHelper
    {
        internal StrideHelper()
        {
            Lengths = new List<int>();
            Offsets = new List<int>();
        }

        internal IList<int> Lengths { get; }

        internal IList<int> Offsets { get; }

        internal int Total { get; set; }
    }
}