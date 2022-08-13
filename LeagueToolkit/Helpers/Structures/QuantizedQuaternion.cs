using System.Numerics;

namespace LeagueToolkit.Helpers.Structures;

public struct QuantizedQuaternion
{
    private readonly ushort[] _data;

    public QuantizedQuaternion(byte[] data)
    {
        _data = new[]
        {
            (ushort)(data[0] | (data[1] << 8)),
            (ushort)(data[2] | (data[3] << 8)),
            (ushort)(data[4] | (data[5] << 8))
        };
    }

    public Quaternion Decompress()
    {
        var bits = _data[0] | ((ulong)_data[1] << 16) | ((ulong)_data[2] << 32);
        var maxIndex = (ushort)((bits >> 45) & 0x0003u);
        var v_a = (ushort)((bits >> 30) & 0x7FFFu);
        var v_b = (ushort)((bits >> 15) & 0x7FFFu);
        var v_c = (ushort)(bits & 0x7FFFu);

        var sqrt2 = 1.41421356237;
        var a = (float)(v_a / 32767.0 * sqrt2 - 1 / sqrt2);
        var b = (float)(v_b / 32767.0 * sqrt2 - 1 / sqrt2);
        var c = (float)(v_c / 32767.0 * sqrt2 - 1 / sqrt2);
        var sub = Math.Max(0, 1 - (a * a + b * b + c * c));
        var d = (float)Math.Sqrt(sub);

        switch (maxIndex)
        {
            case 0: return new Quaternion(d, a, b, c);
            case 1: return new Quaternion(a, d, b, c);
            case 2: return new Quaternion(a, b, d, c);
            default: return new Quaternion(a, b, c, d);
        }
    }
}