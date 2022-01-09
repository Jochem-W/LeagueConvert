using System.Numerics;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.Helpers.Extensions;

internal static class BinaryReaderColorExtensions
{
    public static Color ReadColor(this BinaryReader reader, ColorFormat format)
    {
        if (format == ColorFormat.RgbU8)
        {
            var r = reader.ReadByte() / 255f;
            var g = reader.ReadByte() / 255f;
            var b = reader.ReadByte() / 255f;
            return new Color(r, g, b);
        }

        if (format == ColorFormat.RgbaU8)
        {
            var r = reader.ReadByte() / 255f;
            var g = reader.ReadByte() / 255f;
            var b = reader.ReadByte() / 255f;
            var a = reader.ReadByte() / 255f;
            return new Color(r, g, b, a);
        }

        if (format == ColorFormat.RgbF32)
        {
            var r = reader.ReadSingle();
            var g = reader.ReadSingle();
            var b = reader.ReadSingle();
            return new Color(r, g, b);
        }

        if (format == ColorFormat.RgbaF32)
        {
            var r = reader.ReadSingle();
            var g = reader.ReadSingle();
            var b = reader.ReadSingle();
            var a = reader.ReadSingle();
            return new Color(r, g, b, a);
        }

        if (format == ColorFormat.BgrU8)
        {
            var b = reader.ReadByte() / 255f;
            var g = reader.ReadByte() / 255f;
            var r = reader.ReadByte() / 255f;
            return new Color(r, g, b);
        }

        if (format == ColorFormat.BgraU8)
        {
            var b = reader.ReadByte() / 255f;
            var g = reader.ReadByte() / 255f;
            var r = reader.ReadByte() / 255f;
            var a = reader.ReadByte() / 255f;
            return new Color(r, g, b, a);
        }

        if (format == ColorFormat.BgrF32)
        {
            var b = reader.ReadSingle();
            var g = reader.ReadSingle();
            var r = reader.ReadSingle();
            return new Color(r, g, b);
        }

        if (format == ColorFormat.BgraF32)
        {
            var b = reader.ReadSingle();
            var g = reader.ReadSingle();
            var r = reader.ReadSingle();
            var a = reader.ReadSingle();
            return new Color(r, g, b, a);
        }

        throw new ArgumentException("Unsupported format", nameof(format));
    }

    public static Vector2 ReadVector2(this BinaryReader reader)
    {
        return new Vector2(reader.ReadSingle(), reader.ReadSingle());
    }

    public static Vector3 ReadVector3(this BinaryReader reader)
    {
        return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }

    public static Vector4 ReadVector4(this BinaryReader reader)
    {
        return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }

    public static Quaternion ReadQuaternion(this BinaryReader reader)
    {
        return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }

    public static Matrix4x4 ReadMatrix4x4RowMajor(this BinaryReader reader)
    {
        return new Matrix4x4
        {
            M11 = reader.ReadSingle(),
            M12 = reader.ReadSingle(),
            M13 = reader.ReadSingle(),
            M14 = reader.ReadSingle(),
            M21 = reader.ReadSingle(),
            M22 = reader.ReadSingle(),
            M23 = reader.ReadSingle(),
            M24 = reader.ReadSingle(),
            M31 = reader.ReadSingle(),
            M32 = reader.ReadSingle(),
            M33 = reader.ReadSingle(),
            M34 = reader.ReadSingle(),
            M41 = reader.ReadSingle(),
            M42 = reader.ReadSingle(),
            M43 = reader.ReadSingle(),
            M44 = reader.ReadSingle()
        };
    }

    public static string ReadPaddedString(this BinaryReader reader, int length)
    {
        return new string(reader.ReadChars(length)).Replace("\0", string.Empty);
    }

    public static string ReadZeroTerminatedString(this BinaryReader reader)
    {
        var returnString = "";
        while (true)
        {
            var c = reader.ReadChar();
            if (c == '\0') break;

            returnString += c;
        }

        return returnString;
    }
}