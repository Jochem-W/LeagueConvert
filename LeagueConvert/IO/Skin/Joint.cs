using System.Numerics;
using LeagueToolkit.IO.SkeletonFile;

namespace LeagueConvert.IO.Skin;

public class Joint
{
    private readonly SkeletonJoint _joint;

    public Joint(SkeletonJoint joint)
    {
        _joint = joint;
        LocalTransform = joint.LocalTransform;
        InverseBindTransform = joint.InverseBindTransform;
        GlobalTransform = joint.GlobalTransform;
    }

    public int Id => _joint.Id;

    public int ParentId => _joint.ParentId;

    public Matrix4x4 InverseBindTransform { get; internal set; }

    public Matrix4x4 LocalTransform { get; internal set; }

    public Matrix4x4 GlobalTransform { get; internal set; }

    public string Name => _joint.Name;
}