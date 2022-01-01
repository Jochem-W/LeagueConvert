using System.IO;
using System.Numerics;
using LeagueToolkit.Helpers.Extensions;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeVector3 : BinTreeProperty
{
    public BinTreeVector3(IBinTreeParent parent, uint nameHash, Vector3 value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeVector3(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadVector3();
    }

    public override BinPropertyType Type => BinPropertyType.Vector3;
    public Vector3 Value { get; set; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.WriteVector3(Value);
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = includeHeader ? 5 : 0;
        return size + 12;
    }

    public override bool Equals(BinTreeProperty other)
    {
        return other is BinTreeVector3 property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator Vector3(BinTreeVector3 property)
    {
        return property.Value;
    }
}