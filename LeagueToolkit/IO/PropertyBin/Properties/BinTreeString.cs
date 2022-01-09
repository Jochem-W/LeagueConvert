using System.Text;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeString : BinTreeProperty
{
    public BinTreeString(IBinTreeParent parent, uint nameHash, string value) : base(parent, nameHash)
    {
        Value = value;
    }

    internal BinTreeString(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        Value = Encoding.ASCII.GetString(br.ReadBytes(br.ReadUInt16()));
    }

    public override BinPropertyType Type => BinPropertyType.String;

    public string Value { get; set; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.Write((ushort) Value.Length);
        bw.Write(Encoding.UTF8.GetBytes(Value));
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = includeHeader ? 5 : 0;
        return size + 2 + Value.Length;
    }

    public override bool Equals(BinTreeProperty other)
    {
        return other is BinTreeString property
               && NameHash == property.NameHash
               && Value == property.Value;
    }

    public static implicit operator string(BinTreeString property)
    {
        return property.Value;
    }
}