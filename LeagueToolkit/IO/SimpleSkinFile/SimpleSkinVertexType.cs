namespace LeagueToolkit.IO.SimpleSkinFile;

public enum SimpleSkinVertexType
{
    Basic,
    Color,
    ColorAndTangent
}

public static class SimpleSkinVertexTypeExtensions
{
    public static bool IsDefinedFast(this SimpleSkinVertexType simpleSkinVertexType)
    {
        return simpleSkinVertexType is >= SimpleSkinVertexType.Basic and <= SimpleSkinVertexType.ColorAndTangent;
    }
}