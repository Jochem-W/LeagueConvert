using System.Collections.Generic;
using LeagueToolkit.IO.OBJ;

namespace LeagueToolkit.IO.MapGeometry;

public static class MapGeometryOBJExtensions
{
    public static (List<ushort>, List<MapGeometryVertex>) GetMGEOData(this OBJFile obj)
    {
        var indices = new List<ushort>();
        var vertices = new List<MapGeometryVertex>();

        foreach (var vertex in obj.Vertices) vertices.Add(new MapGeometryVertex {Position = vertex});

        foreach (var face in obj.Faces)
        {
            for (var i = 0; i < 3; i++) indices.Add((ushort) face.VertexIndices[i]);

            if (face.NormalIndices != null)
                for (var i = 0; i < 3; i++)
                    vertices[(int) face.VertexIndices[i]].Normal = obj.Normals[(int) face.NormalIndices[i]];

            if (face.UVIndices != null)
                for (var i = 0; i < 3; i++)
                    vertices[(int) face.VertexIndices[i]].DiffuseUV = obj.UVs[(int) face.UVIndices[i]];
        }

        return (indices, vertices);
    }
}