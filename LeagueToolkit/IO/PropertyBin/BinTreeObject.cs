using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using LeagueToolkit.Helpers.Hashing;

namespace LeagueToolkit.IO.PropertyBin;

public class BinTreeObject : IBinTreeParent, IEquatable<BinTreeObject>
{
    private readonly List<BinTreeProperty> _properties = new();

    internal BinTreeObject(uint metaClassHash)
    {
        MetaClassHash = metaClassHash;
        Properties = _properties.AsReadOnly();
    }

    public BinTreeObject(string metaClass, string path, ICollection<BinTreeProperty> properties)
        : this(Fnv1a.HashLower(metaClass), Fnv1a.HashLower(path), properties)
    {
    }

    public BinTreeObject(string metaClass, uint pathHash, ICollection<BinTreeProperty> properties)
        : this(Fnv1a.HashLower(metaClass), pathHash, properties)
    {
    }

    public BinTreeObject(uint metaClassHash, string path, ICollection<BinTreeProperty> properties)
        : this(metaClassHash, Fnv1a.HashLower(path), properties)
    {
    }

    public BinTreeObject(uint metaClassHash, uint pathHash, ICollection<BinTreeProperty> properties)
    {
        MetaClassHash = metaClassHash;
        PathHash = pathHash;
        _properties = new List<BinTreeProperty>(properties.Select(x =>
        {
            // Assign this as a parent of the properties
            x.Parent = this;
            return x;
        }));
        Properties = _properties.AsReadOnly();
    }

    public uint MetaClassHash { get; }
    public uint PathHash { get; private set; }

    public ReadOnlyCollection<BinTreeProperty> Properties { get; }

    public bool Equals(BinTreeObject other)
    {
        return PathHash == other.PathHash &&
               MetaClassHash == other.MetaClassHash &&
               _properties.SequenceEqual(other._properties);
    }

    internal void ReadData(BinaryReader br)
    {
        var size = br.ReadUInt32();
        PathHash = br.ReadUInt32();

        var propertyCount = br.ReadUInt16();
        for (var i = 0; i < propertyCount; i++) _properties.Add(BinTreeProperty.Read(br, this));
    }

    internal void WriteHeader(BinaryWriter bw)
    {
        bw.Write(MetaClassHash);
    }

    internal void WriteContent(BinaryWriter bw)
    {
        bw.Write(GetSize());
        bw.Write(PathHash);

        bw.Write((ushort) _properties.Count);
        foreach (var property in _properties) property.Write(bw, true);
    }

    public void AddProperty(BinTreeProperty property)
    {
        if (_properties.Any(x => x.NameHash == property.NameHash))
            throw new InvalidOperationException("A property with the same name already exists");

        property.Parent = this;
        _properties.Add(property);
    }

    public void RemoveProperty(uint nameHash)
    {
        if (_properties.FirstOrDefault(x => x.NameHash == nameHash) is BinTreeProperty property)
            _properties.Remove(property);
        else throw new ArgumentException("Failed to find a property with the specified name hash", nameof(nameHash));
    }

    private int GetSize()
    {
        var size = 4 + 2;
        foreach (var property in _properties) size += property.GetSize(true);

        return size;
    }
}