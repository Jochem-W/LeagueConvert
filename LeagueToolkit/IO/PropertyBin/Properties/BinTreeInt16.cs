namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeInt16 : BinTreeProperty
{
    public BinTreeInt16(IBinTreeParent parent, uint nameHash, short value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeInt16(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadInt16();
    }

    public override BinPropertyType Type => BinPropertyType.Int16;
    public short Value { get; set; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.Write(Value);
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = includeHeader ? 5 : 0;
        return size + 2;
    }

    public override bool Equals(BinTreeProperty other)
    {
        return other is BinTreeInt16 property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator short(BinTreeInt16 property)
    {
        return property.Value;
    }
}