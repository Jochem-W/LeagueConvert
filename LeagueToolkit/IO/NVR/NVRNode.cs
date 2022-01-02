using System.Numerics;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.NVR;

public class NVRNode
{
    public static readonly float NullCoordinate = BitConverter.ToSingle(new byte[4] {255, 255, 127, 255}, 0);

    //Values used when writing
    public R3DBox CentralPointsBoundingBox;
    public int ChildNodeCount;
    public int FirstChildNode;

    //Values used when reading
    public int FirstMesh;
    public int MeshCount;

    public NVRNode(BinaryReader br, NVRBuffers buffers)
    {
        BoundingBox = new R3DBox(br);
        FirstMesh = br.ReadInt32();
        MeshCount = br.ReadInt32();
        FirstChildNode = br.ReadInt32();
        ChildNodeCount = br.ReadInt32();

        if (FirstChildNode == -1)
            for (var i = FirstMesh; i < FirstMesh + MeshCount; i++)
                Meshes.Add(buffers.Meshes[i]);
    }

    public NVRNode(R3DBox centralPointsBox, NVRNode parentNode)
    {
        // Used if we create it from a parent node.
        CentralPointsBoundingBox = centralPointsBox;
        Meshes.AddRange(
            parentNode.Meshes.FindAll(x => CentralPointsBoundingBox.ContainsPoint(x.BoundingSphere.Position)));
        parentNode.Children.Add(this);
        BoundingBox = CalculateBoundingBox();
    }

    public NVRNode(List<NVRMesh> meshes)
    {
        // Used when creating the big parent node.
        Meshes.AddRange(meshes);
        BoundingBox = CalculateBoundingBox();
        CentralPointsBoundingBox = CalculateCentralPointsBoundingBox();
    }

    public R3DBox BoundingBox { get; }
    public List<NVRNode> Children { get; } = new();
    public List<NVRMesh> Meshes { get; } = new();

    private R3DBox CalculateBoundingBox()
    {
        if (Meshes.Count > 0)
        {
            var min = new Vector3(Meshes[0].BoundingBox.Min.X, Meshes[0].BoundingBox.Min.Y,
                Meshes[0].BoundingBox.Min.Z);
            var max = new Vector3(Meshes[0].BoundingBox.Max.X, Meshes[0].BoundingBox.Max.Y,
                Meshes[0].BoundingBox.Max.Z);
            for (var i = 1; i < Meshes.Count; i++)
            {
                var box = Meshes[i].BoundingBox;
                if (box.Min.X < min.X) min.X = box.Min.X;
                if (box.Min.Y < min.Y) min.Y = box.Min.Y;
                if (box.Min.Z < min.Z) min.Z = box.Min.Z;
                if (box.Max.X > max.X) max.X = box.Max.X;
                if (box.Max.Y > max.Y) max.Y = box.Max.Y;
                if (box.Max.Z > max.Z) max.Z = box.Max.Z;
            }

            return new R3DBox(min, max);
        }

        // No meshes inside, set bounding box to 
        return new R3DBox(new Vector3(NullCoordinate, NullCoordinate, NullCoordinate),
            new Vector3(NullCoordinate, NullCoordinate, NullCoordinate));
    }

    private R3DBox CalculateCentralPointsBoundingBox()
    {
        if (Meshes.Count > 0)
        {
            var min = new Vector3(Meshes[0].BoundingSphere.Position.X, Meshes[0].BoundingSphere.Position.Y,
                Meshes[0].BoundingSphere.Position.Z);
            var max = new Vector3(Meshes[0].BoundingSphere.Position.X, Meshes[0].BoundingSphere.Position.Y,
                Meshes[0].BoundingSphere.Position.Z);
            for (var i = 1; i < Meshes.Count; i++)
            {
                var spherePosition = Meshes[i].BoundingSphere.Position;
                if (spherePosition.X < min.X) min.X = spherePosition.X;
                if (spherePosition.Y < min.Y) min.Y = spherePosition.Y;
                if (spherePosition.Z < min.Z) min.Z = spherePosition.Z;
                if (spherePosition.X > max.X) max.X = spherePosition.X;
                if (spherePosition.Y > max.Y) max.Y = spherePosition.Y;
                if (spherePosition.Z > max.Z) max.Z = spherePosition.Z;
            }

            return new R3DBox(min, max);
        }

        return new R3DBox(new Vector3(NullCoordinate, NullCoordinate, NullCoordinate),
            new Vector3(NullCoordinate, NullCoordinate, NullCoordinate));
    }

    public void Split()
    {
        var pBox = CentralPointsBoundingBox;
        var middleX = (pBox.Min.X + pBox.Max.X) / 2;
        var middleZ = (pBox.Min.Z + pBox.Max.Z) / 2;
        // Node 1 (bottom-left)
        var node1Min = new Vector3(pBox.Min.X, pBox.Min.Y, pBox.Min.Z);
        var node1Max = new Vector3(middleX, pBox.Max.Y, middleZ);
        var node1 = new NVRNode(new R3DBox(node1Min, node1Max), this);

        // Node 2 (top-left)
        var node2Min = new Vector3(pBox.Min.X, pBox.Min.Y, middleZ);
        var node2Max = new Vector3(middleX, pBox.Max.Y, pBox.Max.Z);
        var node2 = new NVRNode(new R3DBox(node2Min, node2Max), this);

        // Node 3 (top-right)
        var node3Min = new Vector3(middleX, pBox.Min.Y, middleZ);
        var node3Max = new Vector3(pBox.Max.X, pBox.Max.Y, pBox.Max.Z);
        var node3 = new NVRNode(new R3DBox(node3Min, node3Max), this);

        // Node 4 (bottom-right)
        var node4Min = new Vector3(middleX, pBox.Min.Y, pBox.Min.Z);
        var node4Max = new Vector3(pBox.Max.X, pBox.Max.Y, middleZ);
        var node4 = new NVRNode(new R3DBox(node4Min, node4Max), this);

        foreach (var childNode in Children)
        {
            var proportions = childNode.CentralPointsBoundingBox.GetProportions();
            if (childNode.Meshes.Count > 1 && (proportions.X > 100 || proportions.Z > 100)) childNode.Split();
        }
    }

    public void Write(BinaryWriter bw)
    {
        BoundingBox.Write(bw);
        bw.Write(FirstMesh);
        bw.Write(MeshCount);
        bw.Write(FirstChildNode);
        bw.Write(ChildNodeCount);
    }
}