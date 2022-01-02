using LeagueToolkit.Helpers.Hashing;
using LeagueToolkit.IO.PropertyBin;

namespace LeagueToolkit.Meta.Attributes;

public sealed class MetaPropertyAttribute : Attribute
{
    public MetaPropertyAttribute(string name, BinPropertyType type, string otherClass, BinPropertyType primaryType,
        BinPropertyType secondaryType)
    {
        Name = name;
        NameHash = Fnv1a.HashLower(name);

        OtherClass = otherClass;

        ValueType = type;
        PrimaryType = primaryType;
        SecondaryType = secondaryType;
    }

    public MetaPropertyAttribute(uint nameHash, BinPropertyType type, string otherClass, BinPropertyType primaryType,
        BinPropertyType secondaryType)
    {
        Name = string.Empty;
        NameHash = nameHash;

        OtherClass = otherClass;

        ValueType = type;
        PrimaryType = primaryType;
        SecondaryType = secondaryType;
    }

    public string Name { get; }
    public uint NameHash { get; }

    public string OtherClass { get; }

    public BinPropertyType ValueType { get; }
    public BinPropertyType PrimaryType { get; }
    public BinPropertyType SecondaryType { get; }
}