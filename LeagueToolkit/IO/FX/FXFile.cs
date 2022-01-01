using System.Collections.Generic;
using System.IO;
using System.Text;
using LeagueToolkit.Helpers.Exceptions;

namespace LeagueToolkit.IO.FX;

public class FXFile
{
    public FXFile(string fileLocation)
        : this(File.OpenRead(fileLocation))
    {
    }

    public FXFile(Stream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            for (var i = 0; i < 8; i++) Tracks.Add(new FXTrack(br));

            if (br.BaseStream.Position != br.BaseStream.Length)
            {
                var version = br.ReadUInt32();
                if (version != 1) throw new UnsupportedFileVersionException();

                var flag = br.ReadUInt32();
                var targetBoneCount = br.ReadUInt32();

                if ((flag & 1) == 1)
                    for (var i = 0; i < targetBoneCount; i++)
                    {
                        var targetBone = Encoding.ASCII.GetString(br.ReadBytes(64));
                        TargetBones.Add(targetBone.Remove(targetBone.IndexOfAny(new[] {'\u0000', (char) 0xCD})));
                    }
            }
        }
    }

    public List<FXTrack> Tracks { get; } = new();
    public List<string> TargetBones { get; } = new();

    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    public void Write(Stream stream, bool leaveOpen = false)
    {
        using (var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
        {
            foreach (var track in Tracks) track.Write(bw);
            bw.Write((uint) 1);
            bw.Write(TargetBones.Count != 0 ? 1 : 0);
            bw.Write((uint) Tracks.Count);
            foreach (var targetBone in TargetBones) bw.Write(targetBone.PadRight(64, '\u0000').ToCharArray());
        }
    }
}