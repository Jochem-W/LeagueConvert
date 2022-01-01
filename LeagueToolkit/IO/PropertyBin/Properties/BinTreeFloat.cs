using System.IO;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeFloat : BinTreeProperty
{
    public BinTreeFloat(IBinTreeParent parent, uint nameHash, float value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeFloat(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadSingle();
    }

    public override BinPropertyType Type => BinPropertyType.Float;
    public float Value { get; set; }

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
        return other is BinTreeFloat property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator float(BinTreeFloat property)
    {
        return property.Value;
    }
}