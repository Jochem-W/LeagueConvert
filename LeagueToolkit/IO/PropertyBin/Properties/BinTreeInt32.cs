namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeInt32 : BinTreeProperty
{
    public BinTreeInt32(IBinTreeParent parent, uint nameHash, int value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeInt32(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadInt32();
    }

    public override BinPropertyType Type => BinPropertyType.Int32;
    public int Value { get; set; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.Write(Value);
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = includeHeader ? 5 : 0;
        return size + 4;
    }

    public override bool Equals(BinTreeProperty other)
    {
        return other is BinTreeInt32 property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator int(BinTreeInt32 property)
    {
        return property.Value;
    }
}