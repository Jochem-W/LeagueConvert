using Fantome.Libraries.League.IO.BIN;
using System;

namespace LeagueBulkConvert.Converter
{
    class Material
    {
        public ulong Hash { get; set; }

        public bool IsComplete
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Texture))
                    return false;
                return true;
            }
        }

        public string Name { get; set; }

        public string Texture { get; set; }

        public void Complete(BINEntry entry) => Texture = Utils.FindTexture(entry);

        public Material(BINValue material, BINValue submesh, BINValue texture)
        {
            if (submesh is null && texture is null)
                throw new NotImplementedException();
            else if (!(texture is null))
                Texture = ((string)texture.Value).ToLower().Replace('/', '\\');
            if (!(material is null))
                Hash = (uint)material.Value;
            Name = ((string)submesh.Value).ToLower();
        }
    }
}
