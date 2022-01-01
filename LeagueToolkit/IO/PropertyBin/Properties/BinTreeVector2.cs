using System.IO;
using System.Numerics;
using LeagueToolkit.Helpers.Extensions;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeVector2 : BinTreeProperty
{
    public BinTreeVector2(IBinTreeParent parent, uint nameHash, Vector2 value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeVector2(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadVector2();
    }

    public override BinPropertyType Type => BinPropertyType.Vector2;
    public Vector2 Value { get; set; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.WriteVector2(Value);
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = includeHeader ? 5 : 0;
        return size + 8;
    }

    public override bool Equals(BinTreeProperty other)
    {
        return other is BinTreeVector2 property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator Vector2(BinTreeVector2 property)
    {
        return property.Value;
    }
}