namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeInt64 : BinTreeProperty
{
    public BinTreeInt64(IBinTreeParent parent, uint nameHash, long value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeInt64(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadInt64();
    }

    public override BinPropertyType Type => BinPropertyType.Int64;
    public long Value { get; set; }

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
        return other is BinTreeInt64 property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator long(BinTreeInt64 property)
    {
        return property.Value;
    }
}