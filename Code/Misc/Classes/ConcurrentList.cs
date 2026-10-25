using System.Collections;
using System.Collections.ObjectModel;

namespace TssUtils.Code.Misc.Classes;

public class ConcurrentList<T> : IList<T>, IReadOnlyCollection<T>
{
    private readonly List<T> _items = [];
    private readonly object _lock = new();

    public ConcurrentList()
    {
    }

    public ConcurrentList(int size)
    {
        _items = new List<T>(size);
    }

    public T this[int index]
    {
        get
        {
            lock (_lock)
            {
                return _items[index];
            }
        }
        set
        {
            lock (_lock)
            {
                _items[index] = value;
            }
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        lock (_lock)
        {
            return ((IEnumerable<T>)[.. _items]).GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _items.Count;
            }
        }
    }

    public bool IsReadOnly => false;

    public void Add(T item)
    {
        lock (_lock)
        {
            _items.Add(item);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _items.Clear();
        }
    }

    public bool Contains(T item)
    {
        lock (_lock)
        {
            return _items.Contains(item);
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (_lock)
        {
            _items.CopyTo(array, arrayIndex);
        }
    }

    public bool Remove(T item)
    {
        lock (_lock)
        {
            return _items.Remove(item);
        }
    }

    public int IndexOf(T item)
    {
        lock (_lock)
        {
            return _items.IndexOf(item);
        }
    }

    public void Insert(int index, T item)
    {
        lock (_lock)
        {
            _items.Insert(index, item);
        }
    }

    public void RemoveAt(int index)
    {
        lock (_lock)
        {
            _items.RemoveAt(index);
        }
    }

    public void AddRange(IEnumerable<T> enumerable)
    {
        lock (_lock)
        {
            _items.AddRange(enumerable);
        }
    }

    public ReadOnlyCollection<T> AsReadOnly()
    {
        lock (_lock)
        {
            return new ReadOnlyCollection<T>([.. _items]);
        }
    }

    public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
    {
        lock (_lock)
        {
            return _items.BinarySearch(index, count, item, comparer);
        }
    }

    public int BinarySearch(T item)
    {
        lock (_lock)
        {
            return _items.BinarySearch(item);
        }
    }

    public int BinarySearch(T item, IComparer<T> comparer)
    {
        lock (_lock)
        {
            return _items.BinarySearch(item, comparer);
        }
    }
}
