using LeagueToolkit.Helpers.Hashing;

namespace LeagueToolkit.Meta.Attributes;

public sealed class MetaClassAttribute : Attribute
{
    public string _name;
    private uint _nameHash;
    public string _path;
    private uint _pathHash;

    public MetaClassAttribute(string name)
    {
        Name = name;
        _nameHash = Fnv1a.HashLower(name);
    }

    public MetaClassAttribute(uint nameHash)
    {
        Name = string.Empty;
        _nameHash = nameHash;
    }

    public string Name
    {
        get => _name;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _name = value;
            _nameHash = Fnv1a.HashLower(value);
        }
    }

    public uint NameHash
    {
        get => _nameHash;
        set
        {
            _name = null;
            _nameHash = value;
        }
    }

    public string Path
    {
        get => _path;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _path = value;
            _pathHash = Fnv1a.HashLower(value);
        }
    }

    public uint PathHash
    {
        get => _pathHash;
        set
        {
            _path = null;
            _pathHash = value;
        }
    }
}