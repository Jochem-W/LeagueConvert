using System.IO;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeBitBool : BinTreeProperty
{
    public BinTreeBitBool(IBinTreeParent parent, uint nameHash, byte value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeBitBool(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadByte();
    }

    public override BinPropertyType Type => BinPropertyType.BitBool;
    public byte Value { get; set; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.Write(Value);
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = includeHeader ? HEADER_SIZE : 0;
        return size + 1;
    }

    public override bool Equals(BinTreeProperty other)
    {
        return other is BinTreeBitBool property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator byte(BinTreeBitBool property)
    {
        return property.Value;
    }
}