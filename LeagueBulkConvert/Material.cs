﻿using LeagueToolkit.IO.PropertyBin;
using LeagueToolkit.IO.PropertyBin.Properties;

namespace LeagueBulkConvert;

internal class Material
{
    public Material(BinTreeProperty materialProperty, BinTreeProperty submeshProperty,
        BinTreeProperty textureProperty)
    {
        if (materialProperty != null)
            Hash = ((BinTreeObjectLink) materialProperty).Value;
        if (submeshProperty != null)
            Name = ((BinTreeString) submeshProperty).Value.ToLower();
        if (textureProperty != null)
            Texture = ((BinTreeString) textureProperty).Value.ToLower();
    }

    public ulong Hash { get; set; }

    public bool IsComplete => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Texture);

    public string Name { get; set; }

    public string Texture { get; set; }

    public void Complete(BinTreeObject treeObject, Config config)
    {
        if (Utils.FindTexture(treeObject, config, out var texture))
            Texture = texture;
    }
}