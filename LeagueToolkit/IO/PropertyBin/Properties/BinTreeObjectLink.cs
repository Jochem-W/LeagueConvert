namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeObjectLink : BinTreeProperty
{
    public BinTreeObjectLink(IBinTreeParent parent, uint nameHash, uint value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeObjectLink(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadUInt32();
    }

    public override BinPropertyType Type => BinPropertyType.ObjectLink;
    public uint Value { get; set; }

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
        return other is BinTreeObjectLink property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator uint(BinTreeObjectLink property)
    {
        return property.Value;
    }
}