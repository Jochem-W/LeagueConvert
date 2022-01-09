namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeUInt16 : BinTreeProperty
{
    public BinTreeUInt16(IBinTreeParent parent, uint nameHash, ushort value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeUInt16(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadUInt16();
    }

    public override BinPropertyType Type => BinPropertyType.UInt16;
    public ushort Value { get; set; }

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
        return other is BinTreeUInt16 property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator ushort(BinTreeUInt16 property)
    {
        return property.Value;
    }
}