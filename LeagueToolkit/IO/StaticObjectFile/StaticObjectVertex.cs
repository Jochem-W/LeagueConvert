using System.Numerics;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.StaticObjectFile;

public class StaticObjectVertex
{
    public StaticObjectVertex(Vector3 position, Vector2 uv)
    {
        Position = position;
        UV = uv;
    }

    public StaticObjectVertex(Vector3 position, Vector2 uv, Color color) : this(position, uv)
    {
        Color = color;
    }

    public Vector3 Position { get; set; }
    public Vector2 UV { get; set; }
    public Color? Color { get; set; }
}