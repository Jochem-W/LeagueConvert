using System.Numerics;
using System.Text;
using LeagueToolkit.Helpers.Extensions;

namespace LeagueToolkit.IO.FX;

public class FXTrack
{
    public FXTrack(BinaryReader br)
    {
        Flag = br.ReadUInt32();
        Type = (TrackType)br.ReadUInt32();
        StartFrame = br.ReadSingle();
        EndFrame = br.ReadSingle();

        Particle = Encoding.ASCII.GetString(br.ReadBytes(64));
        Bone = Encoding.ASCII.GetString(br.ReadBytes(64));

        Particle = Particle.Remove(Particle.IndexOf(Particle.Contains("\0") ? '\u0000' : '?'));
        Bone = Bone.Remove(Bone.IndexOf(Bone.Contains("\0") ? '\u0000' : '?'));

        SpawnOffset = br.ReadVector3();
        StreakInfo = new FXWeaponStreakInfo(br);
    }

    public uint Flag { get; }
    public TrackType Type { get; }
    public float StartFrame { get; }
    public float EndFrame { get; }
    public string Particle { get; }
    public string Bone { get; }
    public Vector3 SpawnOffset { get; }
    public FXWeaponStreakInfo StreakInfo { get; }

    public void Write(BinaryWriter bw)
    {
        bw.Write(Flag);
        bw.Write((uint)Type);
        bw.Write(StartFrame);
        bw.Write(EndFrame);
        bw.Write(Particle.PadRight(64, '\u0000').ToCharArray());
        bw.Write(Bone.PadRight(64, '\u0000').ToCharArray());
        bw.WriteVector3(SpawnOffset);
        StreakInfo.Write(bw);
    }
}

public enum TrackType : uint
{
    None,
    EffPosition,
    EffBone,
    WStreak
}