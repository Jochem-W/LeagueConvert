namespace SimpleGltf.Extensions
{
    public static class OffsetExtensions
    {
        public static int Offset(this int value, int alignment = 4)
        {
            return value + value.GetOffset(alignment);
        }

        public static long Offset(this long value, int alignment = 4)
        {
            return value + value.GetOffset(alignment);
        }

        public static int GetOffset(this int value, int alignment = 4)
        {
            var offset = alignment - value % alignment;
            return offset == alignment ? 0 : offset;
        }

        public static long GetOffset(this long value, int alignment = 4)
        {
            var offset = alignment - value % alignment;
            return offset == alignment ? 0 : offset;
        }
    }
}