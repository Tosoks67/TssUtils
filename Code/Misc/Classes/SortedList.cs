using System.Collections;

namespace TssUtils.Code.Misc.Classes;

public class SortedList<T>(List<T> list, IComparer<T> comparer) : IList<T>
{
    public SortedList(IComparer<T> comparer) : this([], comparer)
    {
    }

    public SortedList() : this([], null!)
    {
    }

    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(T item)
    {
        list.Add(item);
        list.Sort(comparer);
    }

    public void Clear()
    {
        list.Clear();
    }

    public bool Contains(T item) => list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex)
    {
        list.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        var result = list.Remove(item);
        list.Sort(comparer);
        return result;
    }

    public int Count => list.Count;
    public bool IsReadOnly => false;

    public int IndexOf(T item) => list.IndexOf(item);

    public void Insert(int index, T item)
    {
        list.Insert(index, item);
        list.Sort(comparer);
    }

    public void RemoveAt(int index)
    {
        list.RemoveAt(index);
        list.Sort(comparer);
    }

    public T this[int index]
    {
        get => list[index];
        set
        {
            list[index] = value;
            list.Sort(comparer);
        }
    }

    public void AddRange(IEnumerable<T> collection)
    {
        list.AddRange(collection);
        list.Sort(comparer);
    }
}
