using System.IO;
using System.Numerics;
using LeagueToolkit.Helpers.Extensions;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeMatrix44 : BinTreeProperty
{
    public BinTreeMatrix44(IBinTreeParent parent, uint nameHash, Matrix4x4 value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeMatrix44(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadMatrix4x4RowMajor();
    }

    public override BinPropertyType Type => BinPropertyType.Matrix44;
    public Matrix4x4 Value { get; set; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.WriteMatrix4x4RowMajor(Value);
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = includeHeader ? 5 : 0;
        return size + 64;
    }

    public override bool Equals(BinTreeProperty other)
    {
        return other is BinTreeMatrix44 property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator Matrix4x4(BinTreeMatrix44 property)
    {
        return property.Value;
    }
}