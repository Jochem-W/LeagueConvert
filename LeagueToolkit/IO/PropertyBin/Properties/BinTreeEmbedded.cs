namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeEmbedded : BinTreeStructure
{
    public BinTreeEmbedded(IBinTreeParent parent, uint nameHash, uint metaClassHash,
        IEnumerable<BinTreeProperty> properties)
        : base(parent, nameHash, metaClassHash, properties)
    {
    }

    internal BinTreeEmbedded(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(br, parent, nameHash)
    {
    }

    public override BinPropertyType Type => BinPropertyType.Embedded;

    public override bool Equals(BinTreeProperty other)
    {
        if (NameHash != other.NameHash) return false;

        if (other is BinTreeEmbedded otherProperty)
        {
            if (MetaClassHash != otherProperty.MetaClassHash) return false;
            if (_properties.Count != otherProperty._properties.Count) return false;

            for (var i = 0; i < _properties.Count; i++)
                if (!_properties[i].Equals(otherProperty._properties[i]))
                    return false;
        }

        return true;
    }
}