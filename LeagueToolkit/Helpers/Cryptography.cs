namespace LeagueToolkit.Helpers.Cryptography;

public static class Cryptography
{
    /// <summary>
    ///     Hashes a section and property
    /// </summary>
    /// <param name="section">The section of the hash</param>
    /// <param name="property">The property of the hash</param>
    /// <returns>A hash generated from <paramref name="section" /> and <paramref name="property" /></returns>
    /// <remarks>Used in Inibin</remarks>
    public static uint SectionHash(string section, string property)
    {
        uint hash = 0;
        section = section.ToLower();
        property = property.ToLower();
        for (var i = 0; i < section.Length; i++) hash = section[i] + 65599 * hash;
        hash = 65599 * hash + 42;
        for (var i = 0; i < property.Length; i++) hash = property[i] + 65599 * hash;
        return hash;
    }

    /// <summary>
    ///     Hashes a string
    /// </summary>
    /// <param name="toHash">The string to hash</param>
    /// <returns>A hash generated from <paramref name="toHash" /></returns>
    /// <remarks>Used in RAF, SKL and ANM</remarks>
    public static uint ElfHash(string toHash)
    {
        toHash = toHash.ToLower();

        uint hash = 0;
        uint high = 0;
        for (var i = 0; i < toHash.Length; i++)
        {
            hash = (hash << 4) + (byte) toHash[i];

            if ((high = hash & 0xF0000000) != 0) hash ^= high >> 24;

            hash &= ~high;
        }

        return hash;
    }
}