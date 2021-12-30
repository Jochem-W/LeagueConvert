namespace LeagueToolkit.Meta;

public struct MetaBitBool
{
    public byte Value { get; set; }

    public MetaBitBool(bool value)
    {
        Value = (byte) (value ? 1 : 0);
    }

    public MetaBitBool(byte value)
    {
        Value = value;
    }

    public static implicit operator byte(MetaBitBool bitBool)
    {
        return bitBool.Value;
    }

    public static implicit operator bool(MetaBitBool bitBool)
    {
        return bitBool.Value == 1 ? true : false;
    }
}