using LeagueConvert.Enums;

namespace LeagueConvert.IO.Skin;

public class StaticMaterial
{
    internal StaticMaterial(uint hash)
    {
        Hash = hash;
    }

    public uint Hash { get; }

    public IDictionary<SamplerType, string> Samplers { get; } = new Dictionary<SamplerType, string>();
}