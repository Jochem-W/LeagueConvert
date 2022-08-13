using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.Helpers.Extensions;

internal static class StreamWriterExtensions
{
    public static void WriteColor(this StreamWriter writer, Color color, ColorFormat format)
    {
        writer.Write(color.ToString(format));
    }

    public static void WriteLineIndented(this StreamWriter writer, int indentationLevel, string format,
        params object[] arg)
    {
        for (var i = 0; i < indentationLevel; i++)
        {
            writer.Write("    ");
        }

        writer.WriteLine(format, arg);
    }

    public static void WriteLineIndented(this StreamWriter writer, int indentationLevel, string value)
    {
        for (var i = 0; i < indentationLevel; i++)
        {
            writer.Write("    ");
        }

        writer.WriteLine(value);
    }
}