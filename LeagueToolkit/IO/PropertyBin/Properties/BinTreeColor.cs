using System.IO;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeColor : BinTreeProperty
{
    public BinTreeColor(IBinTreeParent parent, uint nameHash, Color value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeColor(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = br.ReadColor(ColorFormat.RgbaU8);
    }

    public override BinPropertyType Type => BinPropertyType.Color;
    public Color Value { get; set; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.WriteColor(Value, ColorFormat.RgbaU8);
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = includeHeader ? 5 : 0;
        return size + 4;
    }

    public override bool Equals(BinTreeProperty other)
    {
        return other is BinTreeColor property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator Color(BinTreeColor property)
    {
        return property.Value;
    }
}