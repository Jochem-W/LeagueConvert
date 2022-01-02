namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeWadEntryLink : BinTreeProperty
{
    public BinTreeWadEntryLink(IBinTreeParent parent, uint nameHash, ulong value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeWadEntryLink(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadUInt64();
    }

    public override BinPropertyType Type => BinPropertyType.WadEntryLink;

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
        return other is BinTreeWadEntryLink property
               && NameHash == property.NameHash
               && Value == property.Value;
    }
}