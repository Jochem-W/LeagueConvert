using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.NVR;

public class NVRMaterial
{
    public NVRMaterial(BinaryReader br, bool readOld)
    {
        Name = Encoding.ASCII.GetString(br.ReadBytes(260)).Replace("\0", "");
        Type = (NVRMaterialType) br.ReadInt32();
        if (readOld)
        {
            var diffuseColor = br.ReadColor(ColorFormat.RgbaF32);
            var diffuseName = Encoding.ASCII.GetString(br.ReadBytes(260)).Replace("\0", "");
            Channels.Add(new NVRChannel(diffuseName, diffuseColor, R3DMatrix44.IdentityR3DMatrix44()));

            var emmisiveColor = br.ReadColor(ColorFormat.RgbaF32);
            var emissiveName = Encoding.ASCII.GetString(br.ReadBytes(260)).Replace("\0", "");
            Channels.Add(new NVRChannel(emissiveName, emmisiveColor, R3DMatrix44.IdentityR3DMatrix44()));

            for (var i = 0; i < 6; i++)
                Channels.Add(new NVRChannel("", new Color(0, 0, 0, 0), R3DMatrix44.IdentityR3DMatrix44()));
        }
        else
        {
            Flags = (NVRMaterialFlags) br.ReadUInt32();
            for (var i = 0; i < 8; i++) Channels.Add(new NVRChannel(br));
        }
    }

    public NVRMaterial(string name, NVRMaterialType type, NVRMaterialFlags flag, List<NVRChannel> channels)
    {
        Name = name;
        Type = type;
        Flags = flag;
        if (channels.Count != 8) throw new MaterialInvalidChannelCountException(channels.Count);
        Channels.AddRange(channels);
    }

    public string Name { get; }
    public NVRMaterialType Type { get; }
    public NVRMaterialFlags Flags { get; }
    public List<NVRChannel> Channels { get; } = new();

    // Easy way to create a material with working values. Needs to be used with vertex 8
    public static NVRMaterial CreateMaterial(string materialName, string textureName)
    {
        return CreateMaterial(materialName, textureName,
            new Color(0.003921569f, 0.003921569f, 0.003921569f, 0.003921569f), NVRMaterialType.MATERIAL_TYPE_DEFAULT,
            NVRMaterialFlags.ColoredVertex);
    }

    public static NVRMaterial CreateMaterial(string materialName, string textureName, Color color,
        NVRMaterialType matType, NVRMaterialFlags matFlags)
    {
        var channels = new List<NVRChannel>();
        channels.Add(new NVRChannel(textureName, color, R3DMatrix44.IdentityR3DMatrix44()));
        for (var i = 0; i < 7; i++)
            channels.Add(new NVRChannel("", new Color(0, 0, 0, 0), R3DMatrix44.IdentityR3DMatrix44()));
        var newMat = new NVRMaterial(materialName, matType, matFlags, channels);
        return newMat;
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write(Name.PadRight(260, '\u0000').ToCharArray());
        bw.Write((int) Type);
        bw.Write((uint) Flags);
        foreach (var channel in Channels) channel.Write(bw);
    }
}

public enum NVRMaterialType
{
    MATERIAL_TYPE_DEFAULT = 0x0,
    MATERIAL_TYPE_DECAL = 0x1,
    MATERIAL_TYPE_WALL_OF_GRASS = 0x2,
    MATERIAL_TYPE_FOUR_BLEND = 0x3,
    MATERIAL_TYPE_COUNT = 0x4
}

[Flags]
public enum NVRMaterialFlags : uint
{
    GroundVertex = 1,
    ColoredVertex = 16
}

public class MaterialInvalidChannelCountException : Exception
{
    public MaterialInvalidChannelCountException(int actual) : base(
        string.Format("There have to be exactly 8 channels in a material ({0} channel(s) specified).", actual))
    {
    }
}