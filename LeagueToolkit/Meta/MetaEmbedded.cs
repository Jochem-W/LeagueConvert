namespace LeagueToolkit.Meta;

public class MetaEmbedded<T> : IMetaEmbedded where T : IMetaClass
{
    private T _value;

    public MetaEmbedded(T value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        _value = value;
    }

    public T Value
    {
        get => _value;
        set
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            _value = value;
        }
    }

    object IMetaEmbedded.GetValue()
    {
        return Value;
    }

    public static implicit operator T(MetaEmbedded<T> embedded)
    {
        return embedded.Value;
    }
}

internal interface IMetaEmbedded
{
    internal object GetValue();
}