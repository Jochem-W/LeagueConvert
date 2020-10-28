using Fantome.Libraries.League.IO.BIN;
using System;

namespace LeagueBulkConvert.Conversion
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
            if (submesh == null && texture == null)
                throw new NotImplementedException();
            else if (texture != null)
                Texture = ((string)texture.Value).ToLower().Replace('/', '\\');
            if (material != null)
                Hash = (uint)material.Value;
            Name = ((string)submesh.Value).ToLower();
        }
    }
}
