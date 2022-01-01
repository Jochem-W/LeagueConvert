using System.IO;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeSByte : BinTreeProperty
{
    public BinTreeSByte(IBinTreeParent parent, uint nameHash, sbyte value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeSByte(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadSByte();
    }

    public override BinPropertyType Type => BinPropertyType.SByte;
    public sbyte Value { get; set; }

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
        return other is BinTreeSByte property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator sbyte(BinTreeSByte property)
    {
        return property.Value;
    }
}