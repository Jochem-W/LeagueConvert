using LeagueToolkit.Helpers.Hashing;

namespace LeagueToolkit.Meta;

public struct MetaHash
{
    public uint Hash { get; }
    public string Value { get; }

    public MetaHash(uint hash)
    {
        Hash = hash;
        Value = string.Empty;
    }

    public MetaHash(string value)
    {
        Hash = Fnv1a.HashLower(value);
        Value = value;
    }

    public override int GetHashCode()
    {
        return (int) Hash;
    }

    public override bool Equals(object obj)
    {
        return obj is MetaHash other && other.Hash == Hash;
    }

    public static implicit operator uint(MetaHash metaHash)
    {
        return metaHash.Hash;
    }

    public static implicit operator MetaHash(uint hash)
    {
        return new MetaHash(hash);
    }
}