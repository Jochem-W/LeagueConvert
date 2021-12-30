using System.Collections.Generic;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;

namespace LeagueToolkit.IO.StaticObjectFile;

using VERTEX = VertexBuilder<VertexPosition, VertexTexture1, VertexEmpty>;

public static class StaticObjectGltfExtensions
{
    public static ModelRoot ToGltf(this StaticObject staticObject)
    {
        var root = ModelRoot.CreateModel();
        var scene = root.UseScene("default");

        var mesh = VERTEX.CreateCompatibleMesh();

        foreach (var submesh in staticObject.Submeshes)
        {
            var material = new MaterialBuilder(submesh.Name);
            var primitive = mesh.UsePrimitive(material);

            var vertices = new List<VERTEX>();
            foreach (var vertex in submesh.Vertices)
                vertices.Add(new VERTEX()
                    .WithGeometry(vertex.Position)
                    .WithMaterial(vertex.UV));

            for (var i = 0; i < submesh.Indices.Count; i += 3)
            {
                var v1 = vertices[(int) submesh.Indices[i + 0]];
                var v2 = vertices[(int) submesh.Indices[i + 1]];
                var v3 = vertices[(int) submesh.Indices[i + 2]];

                primitive.AddTriangle(v1, v2, v3);
            }
        }

        scene
            .CreateNode()
            .WithMesh(root.CreateMesh(mesh));

        return root;
    }
}