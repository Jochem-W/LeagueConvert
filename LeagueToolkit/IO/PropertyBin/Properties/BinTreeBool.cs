namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeBool : BinTreeProperty
{
    public BinTreeBool(IBinTreeParent parent, uint nameHash, bool value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeBool(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadBoolean();
    }

    public override BinPropertyType Type => BinPropertyType.Bool;

    public bool Value { get; set; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.Write(Value);
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = includeHeader ? 5 : 0;
        return size + 1;
    }

    public override bool Equals(BinTreeProperty other)
    {
        return other is BinTreeBool property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator bool(BinTreeBool property)
    {
        return property.Value;
    }
}