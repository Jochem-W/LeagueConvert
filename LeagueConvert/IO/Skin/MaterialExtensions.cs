using System;
using System.Collections.Generic;
using System.Linq;
using LeagueConvert.IO.WadFile;

namespace LeagueConvert.IO.Skin;

public static class MaterialExtensions
{
    public static void Clean(this IList<Material> materials, StringWad parent)
    {
        for (var i = 0; i < materials.Count; i++)
        {
            var material = materials[i];
            if (parent.EntryExists(material.Texture) && material.SubMesh != null)
                continue;
            materials.RemoveAt(i);
            i--;
        }
    }

    public static IEnumerable<string> GetTextures(this IEnumerable<Material> materials, bool distinct = true)
    {
        var textures = materials.Select(material => material.Texture).Where(texture => texture != null);
        return !distinct ? textures : textures.Distinct();
    }

    public static bool TryGetBySubMesh(this IEnumerable<Material> materials, string subMesh, out Material material)
    {
        material = materials.FirstOrDefault(mat =>
            string.Equals(mat.SubMesh, subMesh, StringComparison.InvariantCultureIgnoreCase));
        return material != null;
    }
}