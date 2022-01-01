using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueToolkit.Helpers.Structures;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;

namespace LeagueToolkit.IO.MapGeometry;

using VERTEX = VertexBuilder<VertexPositionNormal, VertexColor1Texture2, VertexEmpty>;

public static class MapGeometryGltfExtensions
{
    public static ModelRoot ToGLTF(this MapGeometry mgeo)
    {
        var root = ModelRoot.CreateModel();
        var scene = root.UseScene("Map");
        var rootNode = scene.CreateNode("Map");

        // Find all layer combinations used in the Map
        // so we can group the meshes
        var layerModelMap = new Dictionary<MapGeometryLayer, List<MapGeometryModel>>();
        foreach (var model in mgeo.Models)
        {
            if (!layerModelMap.ContainsKey(model.Layer)) layerModelMap.Add(model.Layer, new List<MapGeometryModel>());

            layerModelMap[model.Layer].Add(model);
        }

        // Create node for each layer combination
        var layerNodeMap = new Dictionary<MapGeometryLayer, Node>();
        foreach (var layerModelPair in layerModelMap)
            layerNodeMap.Add(layerModelPair.Key, rootNode.CreateNode(DeriveLayerCombinationName(layerModelPair.Key)));

        foreach (var model in mgeo.Models)
        {
            var meshBuilder = BuildMapGeometryMeshStatic(model);

            layerNodeMap[model.Layer]
                .CreateNode()
                .WithMesh(root.CreateMesh(meshBuilder))
                .WithLocalTransform(new AffineTransform(model.Transformation));
        }

        return root;
    }

    private static string DeriveLayerCombinationName(MapGeometryLayer layerCombination)
    {
        if (layerCombination == MapGeometryLayer.NoLayer) return "NoLayer";

        if (layerCombination == MapGeometryLayer.AllLayers) return "AllLayers";

        var name = "Layer-";

        foreach (MapGeometryLayer layerFlag in Enum.GetValues(typeof(MapGeometryLayer)))
            if (layerCombination.HasFlag(layerFlag) &&
                layerFlag != MapGeometryLayer.AllLayers &&
                layerFlag != MapGeometryLayer.NoLayer)
            {
                var layerIndex = byte.Parse(layerFlag.ToString().Replace("Layer", ""));
                name += layerIndex + "-";
            }

        return name.Remove(name.Length - 1);
    }

    private static IMeshBuilder<MaterialBuilder> BuildMapGeometryMeshStatic(MapGeometryModel model)
    {
        var meshBuilder = VERTEX.CreateCompatibleMesh();

        foreach (var submesh in model.Submeshes)
        {
            var vertices = submesh.GetVertices();
            var indices = submesh.GetIndices();

            var material = new MaterialBuilder(submesh.Material).WithUnlitShader();
            var primitive = meshBuilder.UsePrimitive(material);

            var gltfVertices = new List<VERTEX>();
            foreach (var vertex in vertices) gltfVertices.Add(CreateVertex(vertex));

            for (var i = 0; i < indices.Count; i += 3)
            {
                var v1 = gltfVertices[indices[i + 0]];
                var v2 = gltfVertices[indices[i + 1]];
                var v3 = gltfVertices[indices[i + 2]];

                primitive.AddTriangle(v1, v2, v3);
            }
        }

        return meshBuilder;
    }

    private static VERTEX CreateVertex(MapGeometryVertex vertex)
    {
        var gltfVertex = new VERTEX();

        var position = vertex.Position.Value;
        var normal = vertex.Normal.HasValue ? vertex.Normal.Value : Vector3.Zero;
        var color1 = vertex.SecondaryColor.HasValue ? vertex.SecondaryColor.Value : new Color(0, 0, 0, 1);
        var uv1 = vertex.DiffuseUV.HasValue ? vertex.DiffuseUV.Value : Vector2.Zero;
        var uv2 = vertex.LightmapUV.HasValue ? vertex.LightmapUV.Value : Vector2.Zero;

        return gltfVertex
            .WithGeometry(position, normal)
            .WithMaterial(color1, uv1, uv2);
    }
}