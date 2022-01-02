using System.Collections.ObjectModel;

namespace LeagueToolkit.IO.PropertyBin.Properties;

public sealed class BinTreeMap : BinTreeProperty, IBinTreeParent
{
    private readonly Dictionary<BinTreeProperty, BinTreeProperty> _map = new();

    public BinTreeMap(IBinTreeParent parent, uint nameHash,
        BinPropertyType keyType, BinPropertyType valueType,
        IEnumerable<KeyValuePair<BinTreeProperty, BinTreeProperty>> map)
        : base(parent, nameHash)
    {
        KeyType = keyType;
        ValueType = valueType;

        // Verify property types
        foreach (var pair in map)
        {
            if (pair.Key.Type != keyType)
                throw new ArgumentException("Found a key that does not match the specified key type", nameof(map));
            if (pair.Value.Type != valueType)
                throw new ArgumentException("Found a value that does not match the specified value type", nameof(map));

            pair.Key.Parent = this;
            pair.Value.Parent = this;
        }

        _map = new Dictionary<BinTreeProperty, BinTreeProperty>(map);
        Map = new ReadOnlyDictionary<BinTreeProperty, BinTreeProperty>(_map);
    }

    internal BinTreeMap(BinaryReader br, IBinTreeParent parent, uint nameHash) : base(parent, nameHash)
    {
        KeyType = BinUtilities.UnpackType((BinPropertyType) br.ReadByte());
        ValueType = BinUtilities.UnpackType((BinPropertyType) br.ReadByte());
        var size = br.ReadUInt32();
        var valueCount = br.ReadUInt32();

        for (var i = 0; i < valueCount; i++) _map.Add(Read(br, this, KeyType), Read(br, this, ValueType));

        Map = new ReadOnlyDictionary<BinTreeProperty, BinTreeProperty>(_map);
    }

    public override BinPropertyType Type => BinPropertyType.Map;
    public BinPropertyType KeyType { get; }
    public BinPropertyType ValueType { get; }

    public ReadOnlyDictionary<BinTreeProperty, BinTreeProperty> Map { get; }

    protected override void WriteContent(BinaryWriter bw)
    {
        bw.Write((byte) KeyType);
        bw.Write((byte) BinUtilities.PackType(ValueType));
        bw.Write(GetContentSize());
        bw.Write(_map.Count);

        foreach (var pair in _map)
        {
            pair.Key.Write(bw, false);
            pair.Value.Write(bw, false);
        }
    }

    internal override int GetSize(bool includeHeader)
    {
        return 1 + 1 + 4 + GetContentSize() + (includeHeader ? 5 : 0);
    }

    private int GetContentSize()
    {
        var size = 4;
        foreach (var pair in _map) size += pair.Key.GetSize(false) + pair.Value.GetSize(false);
        return size;
    }

    public void Add(BinTreeProperty key, BinTreeProperty value)
    {
        if (key.Type != KeyType)
            throw new ArgumentException($"Key type ({key.Type}) does not match dictionary key type ({KeyType})",
                nameof(key));
        if (value.Type != ValueType)
            throw new ArgumentException($"Value type ({value.Type}) does not match dictionary value type ({ValueType})",
                nameof(value));

        if (_map.TryAdd(key, value) is false)
            throw new InvalidOperationException(
                "Failed to add the specified key-value pair because the specified key already exists");
    }

    public bool Remove(BinTreeProperty key)
    {
        if (key.Type != KeyType)
            throw new ArgumentException($"Key type ({key.Type}) does not match dictionary key type ({KeyType})",
                nameof(key));

        return _map.Remove(key);
    }

    public override bool Equals(BinTreeProperty other)
    {
        if (NameHash != other.NameHash) return false;

        if (other is BinTreeMap otherProperty)
        {
            if (KeyType != otherProperty.KeyType) return false;
            if (ValueType != otherProperty.ValueType) return false;
            if (_map.Count != otherProperty._map.Count) return false;

            foreach (var entry in _map)
                if (otherProperty._map.TryGetValue(entry.Key, out var value))
                {
                    if (!entry.Value.Equals(value)) return false;
                }
                else
                {
                    return false;
                }
        }

        return true;
    }

    public static implicit operator ReadOnlyDictionary<BinTreeProperty, BinTreeProperty>(BinTreeMap property)
    {
        return property.Map;
    }
}