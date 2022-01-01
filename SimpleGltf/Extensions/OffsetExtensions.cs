namespace SimpleGltf.Extensions
{
    internal static class OffsetExtensions
    {
        internal static int GetOffset(this int value, int alignment)
        {
            var offset = alignment - value % alignment;
            return offset == alignment ? 0 : offset;
        }

        internal static long GetOffset(this long value, int alignment)
        {
            var offset = alignment - value % alignment;
            return offset == alignment ? 0 : offset;
        }
    }
}