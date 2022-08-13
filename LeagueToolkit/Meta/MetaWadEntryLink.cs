namespace LeagueToolkit.Meta;

public struct MetaWadEntryLink
{
    public ulong EntryPathHash { get; }

    public MetaWadEntryLink(ulong entryPathHash)
    {
        EntryPathHash = entryPathHash;
    }

    public override int GetHashCode()
    {
        return (int)EntryPathHash; // ://
    }

    public override bool Equals(object obj)
    {
        return obj is MetaWadEntryLink other && EntryPathHash == other.EntryPathHash;
    }

    public static implicit operator ulong(MetaWadEntryLink wadEntryLink)
    {
        return wadEntryLink.EntryPathHash;
    }

    public static implicit operator MetaWadEntryLink(ulong entryPathHash)
    {
        return new MetaWadEntryLink(entryPathHash);
    }
}