using System.Numerics;

namespace LeagueToolkit.Helpers.Structures;

public struct Color : IEquatable<Color>
{
    public static readonly Color Zero = new(0, 0, 0, 0);

    public float R
    {
        get => _r;
        set
        {
            if (value < 0 || value > 1) throw new ArgumentOutOfRangeException("value", "value must be in 0-1 range");
            _r = value;
        }
    }

    public float G
    {
        get => _g;
        set
        {
            if (value < 0 || value > 1) throw new ArgumentOutOfRangeException("value", "value must be in 0-1 range");
            _g = value;
        }
    }

    public float B
    {
        get => _b;
        set
        {
            if (value < 0 || value > 1) throw new ArgumentOutOfRangeException("value", "value must be in 0-1 range");
            _b = value;
        }
    }

    public float A
    {
        get => _a;
        set
        {
            if (value < 0 || value > 1) throw new ArgumentOutOfRangeException("value", "value must be in 0-1 range");
            _a = value;
        }
    }

    private float _r;
    private float _g;
    private float _b;
    private float _a;

    public Color(byte r, byte g, byte b) : this(r, g, b, 1)
    {
    }

    public Color(byte r, byte g, byte b, byte a)
    {
        _r = r / 255f;
        _g = g / 255f;
        _b = b / 255f;
        _a = a / 255f;
    }

    public Color(float r, float g, float b) : this(r, g, b, 1)
    {
    }

    public Color(float r, float g, float b, float a)
    {
        if (r < 0 || r > 1) Math.Clamp(r, 0, 1);
        if (g < 0 || g > 1) Math.Clamp(g, 0, 1);
        if (b < 0 || b > 1) Math.Clamp(b, 0, 1);
        if (a < 0 || a > 1) Math.Clamp(a, 0, 1);

        _r = r;
        _g = g;
        _b = b;
        _a = a;
    }

    public static int FormatSize(ColorFormat format)
    {
        switch (format)
        {
            case ColorFormat.RgbU8:
            case ColorFormat.BgrU8:
                return 3;
            case ColorFormat.RgbaU8:
            case ColorFormat.BgraU8:
                return 4;
            case ColorFormat.RgbF32:
            case ColorFormat.BgrF32:
                return 12;
            case ColorFormat.RgbaF32:
            case ColorFormat.BgraF32:
                return 16;
            default:
                throw new ArgumentException("Unsupported format", nameof(format));
        }
    }

    public byte[] GetBytes(ColorFormat format)
    {
        var formatSize = FormatSize(format);
        var colorBuffer = new byte[formatSize];

        if (format == ColorFormat.RgbU8)
        {
            colorBuffer[0] = (byte) (R * 255);
            colorBuffer[1] = (byte) (G * 255);
            colorBuffer[2] = (byte) (B * 255);
        }
        else if (format == ColorFormat.RgbaU8)
        {
            colorBuffer[0] = (byte) (R * 255);
            colorBuffer[1] = (byte) (G * 255);
            colorBuffer[2] = (byte) (B * 255);
            colorBuffer[3] = (byte) (A * 255);
        }
        else if (format == ColorFormat.BgrU8)
        {
            colorBuffer[0] = (byte) (B * 255);
            colorBuffer[1] = (byte) (G * 255);
            colorBuffer[2] = (byte) (R * 255);
        }
        else if (format == ColorFormat.BgraU8)
        {
            colorBuffer[0] = (byte) (B * 255);
            colorBuffer[1] = (byte) (G * 255);
            colorBuffer[2] = (byte) (R * 255);
            colorBuffer[3] = (byte) (A * 255);
        }
        else if (format == ColorFormat.RgbF32)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(R), 0, colorBuffer, sizeof(float) * 0, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(G), 0, colorBuffer, sizeof(float) * 1, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(B), 0, colorBuffer, sizeof(float) * 2, sizeof(float));
        }
        else if (format == ColorFormat.RgbaF32)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(R), 0, colorBuffer, sizeof(float) * 0, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(G), 0, colorBuffer, sizeof(float) * 1, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(B), 0, colorBuffer, sizeof(float) * 2, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(A), 0, colorBuffer, sizeof(float) * 3, sizeof(float));
        }
        else if (format == ColorFormat.BgrF32)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(B), 0, colorBuffer, sizeof(float) * 0, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(G), 0, colorBuffer, sizeof(float) * 1, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(R), 0, colorBuffer, sizeof(float) * 2, sizeof(float));
        }
        else if (format == ColorFormat.BgraF32)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(B), 0, colorBuffer, sizeof(float) * 0, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(G), 0, colorBuffer, sizeof(float) * 1, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(R), 0, colorBuffer, sizeof(float) * 2, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(A), 0, colorBuffer, sizeof(float) * 3, sizeof(float));
        }

        return colorBuffer;
    }

    public string ToString(ColorFormat format)
    {
        if (format == ColorFormat.RgbU8)
            return string.Format("{0} {1} {2}", (byte) (R * 255), (byte) (G * 255), (byte) (B * 255));
        if (format == ColorFormat.RgbaU8)
            return string.Format("{0} {1} {2} {3}", (byte) (R * 255), (byte) (G * 255), (byte) (B * 255),
                (byte) (B * 255));
        if (format == ColorFormat.BgrU8)
            return string.Format("{0} {1} {2} {3}", (byte) (B * 255), (byte) (G * 255), (byte) (R * 255),
                (byte) (B * 255));
        if (format == ColorFormat.BgraU8)
            return string.Format("{0} {1} {2} {3}", (byte) (B * 255), (byte) (G * 255), (byte) (R * 255),
                (byte) (B * 255));
        if (format == ColorFormat.RgbF32)
            return string.Format("{0} {1} {2}", R, G, B);
        if (format == ColorFormat.RgbaF32)
            return string.Format("{0} {1} {2} {3}", R, G, B, A);
        if (format == ColorFormat.BgrF32)
            return string.Format("{0} {1} {2}", B, G, R);
        if (format == ColorFormat.BgraF32)
            return string.Format("{0} {1} {2} {3}", B, G, R, A);
        throw new ArgumentException("Unsupported format", nameof(format));
    }
    
    public static implicit operator Vector4(Color color)
    {
        return new Vector4(color.R, color.G, color.B, color.A);
    }

    public bool Equals(Color other)
    {
        return _r.Equals(other._r) && _g.Equals(other._g) && _b.Equals(other._b) && _a.Equals(other._a);
    }

    public override bool Equals(object obj)
    {
        return obj is Color other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_r, _g, _b, _a);
    }

    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Color left, Color right)
    {
        return !left.Equals(right);
    }
}

public enum ColorFormat
{
    RgbU8,
    RgbF32,
    RgbaU8,
    RgbaF32,
    BgrU8,
    BgrF32,
    BgraU8,
    BgraF32
}