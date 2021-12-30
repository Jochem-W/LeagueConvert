using System.Collections.Generic;
using System.Numerics;
using LeagueToolkit.IO.OBJ;

namespace LeagueToolkit.IO.StaticObjectFile;

public static class StaticObjectObjExtensions
{
    public static List<(string MaterialName, OBJFile Obj)> ToObj(this StaticObject staticObject)
    {
        var objs = new List<(string, OBJFile)>();

        foreach (var submesh in staticObject.Submeshes) objs.Add((submesh.Name, submesh.ToObj()));

        return objs;
    }

    public static OBJFile ToObj(this StaticObjectSubmesh submesh)
    {
        var vertices = new List<Vector3>(submesh.Vertices.Count);
        var uvs = new List<Vector2>(submesh.Vertices.Count);
        foreach (var vertex in submesh.Vertices)
        {
            vertices.Add(vertex.Position);
            uvs.Add(vertex.UV);
        }

        return new OBJFile(vertices, submesh.Indices, uvs);
    }
}