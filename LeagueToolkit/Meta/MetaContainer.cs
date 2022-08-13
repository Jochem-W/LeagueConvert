using System.Collections;

namespace LeagueToolkit.Meta;

public class MetaContainer<T> : IList<T>
{
    private readonly List<T> _list = new();

    public MetaContainer()
    {
    }

    public MetaContainer(ICollection<T> items)
    {
        IsFixedSize = false;
        _list = new List<T>(items);
    }

    public MetaContainer(ICollection<T> items, int fixedSize)
    {
        if (items.Count > fixedSize)
        {
            throw new ArgumentException(
                $"{nameof(items.Count)}: {items.Count} is higher than {nameof(fixedSize)}: {fixedSize}");
        }

        IsFixedSize = true;
        FixedSize = fixedSize;
        _list = new List<T>(items);
    }

    public bool IsFixedSize { get; }
    public int FixedSize { get; }
    public int Count => _list.Count;
    public bool IsReadOnly => false;

    public T this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    public void Add(T item)
    {
        // List is full
        if (IsFixedSize && _list.Count == FixedSize)
        {
            throw new InvalidOperationException("maximum list size reached: " + FixedSize);
        }

        _list.Add(item);
    }

    public void Clear()
    {
        _list.Clear();
    }

    public bool Contains(T item)
    {
        return _list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    public int IndexOf(T item)
    {
        return _list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        if (IsFixedSize && index >= FixedSize)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"must be within bounds of {FixedSize}");
        }

        _list.Add(item);
    }

    public bool Remove(T item)
    {
        return _list.Remove(item);
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }
}

public class MetaUnorderedContainer<T> : MetaContainer<T>
{
}