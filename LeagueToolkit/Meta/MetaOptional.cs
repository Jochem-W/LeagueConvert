namespace LeagueToolkit.Meta;

public struct MetaOptional<T> : IMetaOptional
{
    public bool IsSome { get; private set; }

    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            IsSome = value is not null;
        }
    }

    private T _value;

    public MetaOptional(T value, bool isSome)
    {
        IsSome = isSome;
        _value = value;
    }

    object IMetaOptional.GetValue()
    {
        if (IsSome) return _value;
        return null;
    }

    public static implicit operator T(MetaOptional<T> optional)
    {
        return optional.Value;
    }
}

internal interface IMetaOptional
{
    internal object GetValue();
}