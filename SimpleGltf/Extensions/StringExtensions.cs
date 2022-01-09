namespace SimpleGltf.Extensions;

internal static class StringExtensions
{
    internal static uint ToMagic(this string value)
    {
        var chars = value.ToCharArray().Reverse();
        var bytes = chars.Select(character => (byte) character);
        var hex = string.Join(string.Empty, bytes.Select(b => b.ToString("X")));
        var output = Convert.ToUInt32(hex, 16);
        return output;
    }
}