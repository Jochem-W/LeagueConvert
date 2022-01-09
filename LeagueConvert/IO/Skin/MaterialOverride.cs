namespace LeagueConvert.IO.Skin;

public class MaterialOverride
{
    public MaterialOverride(uint? hash = null, string texture = null, string subMesh = null)
    {
        Hash = hash;
        Texture = texture;
        SubMesh = subMesh;
    }

    public uint? Hash { get; }
    public string Texture { get; set; }
    public string SubMesh { get; }
}