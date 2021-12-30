using System.IO;
using System.Numerics;
using LeagueToolkit.Helpers.Extensions;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeVector4 : BinTreeProperty
{
    public BinTreeVector4(IBinTreeParent parent, uint nameHash, Vector4 value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeVector4(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadVector4();
    }

    public override BinPropertyType Type => BinPropertyType.Vector4;
    public Vector4 Value { get; set; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.WriteVector4(Value);
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = includeHeader ? 5 : 0;
        return size + 16;
    }

    public override bool Equals(BinTreeProperty other)
    {
        return other is BinTreeVector4 property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator Vector4(BinTreeVector4 property)
    {
        return property.Value;
    }
}