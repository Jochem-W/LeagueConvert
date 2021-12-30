using System.IO;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeByte : BinTreeProperty
{
    public BinTreeByte(IBinTreeParent parent, uint nameHash, byte value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeByte(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadByte();
    }

    public override BinPropertyType Type => BinPropertyType.Byte;
    public byte Value { get; set; }

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
        return other is BinTreeByte property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator byte(BinTreeByte property)
    {
        return property.Value;
    }
}