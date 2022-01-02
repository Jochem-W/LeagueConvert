namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeUInt64 : BinTreeProperty
{
    public BinTreeUInt64(IBinTreeParent parent, uint nameHash, ulong value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeUInt64(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadUInt64();
    }

    public override BinPropertyType Type => BinPropertyType.UInt64;
    public ulong Value { get; set; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.Write(Value);
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = includeHeader ? 5 : 0;
        return size + 8;
    }

    public override bool Equals(BinTreeProperty other)
    {
        return other is BinTreeUInt64 property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator ulong(BinTreeUInt64 property)
    {
        return property.Value;
    }
}