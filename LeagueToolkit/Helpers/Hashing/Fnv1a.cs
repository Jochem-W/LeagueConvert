namespace LeagueToolkit.Helpers.Hashing;

public static class Fnv1a
{
    public static uint HashLower(string input)
    {
        input = input.ToLower();

        var hash = 2166136261;
        for (var i = 0; i < input.Length; i++)
        {
            hash ^= input[i];
            hash *= 16777619;
        }

        return hash;
    }
}