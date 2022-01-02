using System.Numerics;

namespace LeagueConvert.IO.Skin;

public class Skeleton
{
    public Skeleton(LeagueToolkit.IO.SkeletonFile.Skeleton skeleton)
    {
        Joints = new List<Joint>();
        foreach (var joint in skeleton.Joints)
            Joints.Add(new Joint(joint));
        Influences = skeleton.Influences;
        Calculate();
    }

    public IList<Joint> Joints { get; }

    public IList<short> Influences { get; }

    private void Calculate()
    {
        foreach (var joint in Joints)
        {
            if (!float.IsNaN(joint.InverseBindTransform.M11))
                continue;
            if (float.IsNaN(joint.GlobalTransform.M11))
            {
                var parent = Joints.FirstOrDefault(j => j.Id == joint.ParentId);
                joint.GlobalTransform = parent == null
                    ? Matrix4x4.Identity * joint.LocalTransform
                    : parent.GlobalTransform * joint.LocalTransform;
            }

            Matrix4x4.Invert(joint.GlobalTransform, out var inverse);
            joint.InverseBindTransform = inverse;
        }
    }
}