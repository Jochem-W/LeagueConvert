using System;

namespace LeagueConvert.IO.Skin
{
    public class Material
    {
        public Material(uint? hash = null, string texture = null, string subMesh = null)
        {
            Hash = hash;
            Texture = texture;
            SubMesh = subMesh;
        }

        public uint? Hash { get; }
        public string Texture { get; set; }
        public string SubMesh { get; }

        public void SetTexture(string texture)
        {
            if (Texture != null)
                throw new NotImplementedException();
            Texture = texture;
        }
    }
}