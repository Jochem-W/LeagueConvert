using System.Collections.ObjectModel;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public class BinTreeStructure : BinTreeProperty, IBinTreeParent
{
    protected List<BinTreeProperty> _properties = new();

    public BinTreeStructure(IBinTreeParent parent, uint nameHash, uint metaClassHash,
        IEnumerable<BinTreeProperty> properties)
        : base(parent, nameHash)
    {
        MetaClassHash = metaClassHash;

        // Verify properties
        foreach (var property in properties)
        {
            if (properties.Any(x => x.NameHash == property.NameHash && x != property))
            {
                throw new ArgumentException($"Found two properties with the same name hash: {property.NameHash}");
            }

            property.Parent = this;
        }

        _properties = new List<BinTreeProperty>(properties);
        Properties = _properties.AsReadOnly();
    }

    internal BinTreeStructure(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        MetaClassHash = br.ReadUInt32();
        if (MetaClassHash == 0)
        {
            Properties = _properties.AsReadOnly();
            return; // Empty structure
        }

        var size = br.ReadUInt32();
        var propertyCount = br.ReadUInt16();
        for (var i = 0; i < propertyCount; i++)
        {
            _properties.Add(Read(br, this));
        }

        Properties = _properties.AsReadOnly();
    }

    public override BinPropertyType Type => BinPropertyType.Structure;
    public uint MetaClassHash { get; }

    public ReadOnlyCollection<BinTreeProperty> Properties { get; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.Write(MetaClassHash);
        if (MetaClassHash == 0)
        {
            return; // empty
        }

        bw.Write(GetContentSize());
        bw.Write((ushort)_properties.Count);

        foreach (var property in _properties)
        {
            property.Write(bw, true);
        }
    }

    public void AddProperty(BinTreeProperty property)
    {
        if (_properties.Any(x => x.NameHash == property.NameHash))
        {
            throw new InvalidOperationException("A property with the same name already exists");
        }

        _properties.Add(property);
    }

    public bool RemoveProperty(BinTreeProperty property)
    {
        if (property is not null)
        {
            return _properties.Remove(property);
        }

        return false;
    }

    public bool RemoveProperty(uint nameHash)
    {
        return RemoveProperty(_properties.FirstOrDefault(x => x.NameHash == nameHash));
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = includeHeader ? HEADER_SIZE : 0;
        if (MetaClassHash == 0) // empty struct
        {
            return size + 4;
        }

        return size + 4 + 4 + GetContentSize();
    }

    private int GetContentSize()
    {
        var size = 2;
        foreach (var property in _properties)
        {
            size += property.GetSize(true);
        }

        return size;
    }

    public override bool Equals(BinTreeProperty other)
    {
        if (NameHash != other.NameHash)
        {
            return false;
        }

        if (other is BinTreeStructure otherProperty && other is not BinTreeEmbedded)
        {
            if (MetaClassHash != otherProperty.MetaClassHash)
            {
                return false;
            }

            if (_properties.Count != otherProperty._properties.Count)
            {
                return false;
            }

            for (var i = 0; i < _properties.Count; i++)
            {
                if (!_properties[i].Equals(otherProperty._properties[i]))
                {
                    return false;
                }
            }
        }

        return true;
    }
}