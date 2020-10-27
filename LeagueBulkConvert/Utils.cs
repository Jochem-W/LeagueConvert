using System.Runtime.InteropServices;

namespace LeagueBulkConvert
{
    static class Utils
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);
    }
}
