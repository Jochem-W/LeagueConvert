namespace LeagueToolkit.Meta;

public struct MetaObjectLink
{
    public uint ObjectPathHash { get; }

    public MetaObjectLink(uint objectPathHash)
    {
        ObjectPathHash = objectPathHash;
    }

    public override int GetHashCode()
    {
        return (int)ObjectPathHash;
    }

    public override bool Equals(object obj)
    {
        return obj is MetaObjectLink other && ObjectPathHash == other.ObjectPathHash;
    }

    public static implicit operator uint(MetaObjectLink objectLink)
    {
        return objectLink.ObjectPathHash;
    }

    public static implicit operator MetaObjectLink(uint objectPathHash)
    {
        return new MetaObjectLink(objectPathHash);
    }
}