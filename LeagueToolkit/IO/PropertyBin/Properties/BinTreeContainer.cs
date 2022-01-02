using System.Collections.ObjectModel;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public class BinTreeContainer : BinTreeProperty, IBinTreeParent
{
    protected List<BinTreeProperty> _properties = new();

    public BinTreeContainer(IBinTreeParent parent, uint nameHash, BinPropertyType propertiesType,
        IEnumerable<BinTreeProperty> properties)
        : base(parent, nameHash)
    {
        PropertiesType = propertiesType;

        // Verify properties
        foreach (var property in properties)
        {
            if (property.Type != propertiesType)
                throw new ArgumentException(
                    $"Found a property with a different type ({property.Type}) than the specified one {propertiesType}");

            property.Parent = this;
        }

        _properties = new List<BinTreeProperty>(properties);
        Properties = _properties.AsReadOnly();
    }

    internal BinTreeContainer(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        PropertiesType = BinUtilities.UnpackType((BinPropertyType) br.ReadByte());
        var size = br.ReadUInt32();

        var valueCount = br.ReadUInt32();
        for (var i = 0; i < valueCount; i++) _properties.Add(Read(br, this, PropertiesType));

        Properties = _properties.AsReadOnly();
    }

    public override BinPropertyType Type => BinPropertyType.Container;

    public BinPropertyType PropertiesType { get; }

    public ReadOnlyCollection<BinTreeProperty> Properties { get; }

    protected override void WriteContent(BinaryWriter bw)
    {
        // Verify that all properties have the correct type and parent
        foreach (var property in _properties)
        {
            if (property.Type != PropertiesType)
                throw new InvalidOperationException("Found a Property with an invalid Type");
            if (property.Parent != this) throw new InvalidOperationException("Found a Property with an invalid Parent");
        }

        bw.Write((byte) BinUtilities.PackType(PropertiesType));
        bw.Write(GetContentSize());
        bw.Write(_properties.Count);

        foreach (var property in _properties)
        {
            if (property.Type != PropertiesType)
                throw new InvalidOperationException("Found a Property with an invalid Type");
            if (property.Parent != this) throw new InvalidOperationException("Found a Property with an invalid Parent");

            property.Write(bw, false);
        }
    }

    public void Add(BinTreeProperty property)
    {
        if (property.Type != PropertiesType)
            throw new ArgumentException(
                $"Property type ({property.Type}) does not match container prooperties type ({Type})",
                nameof(property));
        if (_properties.Any(x => x.NameHash == property.NameHash))
            throw new ArgumentException("A Property with the same name hash already exists", nameof(property));
        _properties.Add(property);
    }

    public bool Remove(BinTreeProperty property)
    {
        if (property.Type != PropertiesType) return false;
        return _properties.Remove(property);
    }

    internal override int GetSize(bool includeHeader)
    {
        var size = includeHeader ? 5 : 0;
        return size + 4 + 1 + GetContentSize();
    }

    private int GetContentSize()
    {
        var size = 4;
        foreach (var property in _properties) size += property.GetSize(false);

        return size;
    }

    public override bool Equals(BinTreeProperty other)
    {
        if (NameHash != other.NameHash) return false;

        if (other is BinTreeContainer otherProperty && other is not BinTreeUnorderedContainer)
        {
            if (_properties.Count != otherProperty._properties.Count) return false;
            if (PropertiesType != otherProperty.PropertiesType) return false;

            for (var i = 0; i < _properties.Count; i++)
                if (!_properties[i].Equals(otherProperty._properties[i]))
                    return false;
        }

        return true;
    }
}