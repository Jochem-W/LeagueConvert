using System.IO;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeOptional : BinTreeProperty, IBinTreeParent
{
    public BinTreeOptional(IBinTreeParent parent, uint nameHash, BinPropertyType type, BinTreeProperty value) : base(
        parent, nameHash)
    {
        ValueType = type;
        Value = value;

        if (value is not null) value.Parent = this;
    }

    internal BinTreeOptional(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        ValueType = BinUtilities.UnpackType((BinPropertyType) br.ReadByte());
        var isSome = br.ReadBoolean();

        if (isSome) Value = Read(br, this, ValueType);
    }

    public override BinPropertyType Type => BinPropertyType.Optional;
    public BinPropertyType ValueType { get; }
    public BinTreeProperty Value { get; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.Write((byte) BinUtilities.PackType(ValueType));
        bw.Write(Value is not null);

        if (Value is not null) Value.Write(bw, false);
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = 2 + (includeHeader ? 5 : 0);

        if (Value is not null) size += Value.GetSize(false);

        return size;
    }

    public override bool Equals(BinTreeProperty other)
    {
        if (NameHash != other?.NameHash) return false;

        if (other is BinTreeOptional otherProperty)
        {
            if (ValueType != otherProperty.ValueType) return false;
            return Value is BinTreeProperty value && value.Equals(otherProperty.Value);
        }

        return false;
    }
}