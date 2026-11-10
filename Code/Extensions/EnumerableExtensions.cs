using System.Collections.ObjectModel;
using TssUtils.Code.Classes;

namespace TssUtils.Code.Extensions;

public static class EnumerableExtensions
{
    private static NotSupportedException CollectionReadOnlyEx => new(
        "Collection is read-only"
    );

    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        return source.SelectMany(static x => x);
    }

    public static string JoinStrings(this IEnumerable<string> enumerable, string separator = ", ") =>
        string.Join(separator, enumerable);

    extension<T>(IEnumerable<T> enumerable)
    {
        public IEnumerable<IGrouping<T, T>> Group()
        {
            return enumerable.GroupBy(static x => x);
        }

        public bool ContainsAnyFrom(ICollection<T> other) => enumerable.Any(other.Contains);

        public void ForEach(Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }

        public IEnumerable<T> Tap(Action<T> action)
        {
            using var enumerator = enumerable.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                action(current);
                yield return current;
            }
        }

        public IEnumerable<T> AppendIfAll(T item, Func<T, bool> predicate)
        {
            using var enumerator = enumerable.GetEnumerator();

            var shouldAdd = true;
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (shouldAdd && !predicate(current))
                    shouldAdd = false;
                yield return current;
            }

            if (shouldAdd)
                yield return item;
        }

        public IEnumerable<T> AppendIfAny(T item, Func<T, bool> predicate)
        {
            using var enumerator = enumerable.GetEnumerator();

            var shouldAdd = false;
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (!shouldAdd && predicate(current))
                    shouldAdd = true;
                yield return current;
            }

            if (shouldAdd)
                yield return item;
        }

        public byte Sum(Func<T, byte> selector) => enumerable.Select(selector).Sum();
        public sbyte Sum(Func<T, sbyte> selector) => enumerable.Select(selector).Sum();
        public short Sum(Func<T, short> selector) => enumerable.Select(selector).Sum();
        public ushort Sum(Func<T, ushort> selector) => enumerable.Select(selector).Sum();
        public uint Sum(Func<T, uint> selector) => enumerable.Select(selector).Sum();
        public ulong Sum(Func<T, ulong> selector) => enumerable.Select(selector).Sum();
    }

    extension<T>(ICollection<T> collection)
    {
        public int IndexOf(T other)
        {
            var idx = 0;

            foreach (var item in collection)
            {
                if (EqualityComparer<T>.Default.Equals(item, other))
                    return idx;

                idx++;
            }

            return -1;
        }

        public void AddRange(IEnumerable<T> enumerable)
        {
            if (collection.IsReadOnly)
                throw CollectionReadOnlyEx;

            foreach (var item in enumerable)
                collection.Add(item);
        }

        public void ReplaceWith(IEnumerable<T> enumerable)
        {
            if (collection is T[] array)
            {
                using var enumerator = enumerable.GetEnumerator();
                for (var i = 0; i < array.Length; i++)
                    array[i] = enumerator.MoveNext()
                        ? enumerator.Current
                        : default!;

                return;
            }

            if (collection.IsReadOnly)
                throw CollectionReadOnlyEx;

            var items = enumerable.ToArray();

            collection.Clear();
            foreach (var item in items)
                collection.Add(item);
        }
    }

    extension<T>(IList<T> collection)
    {
        public ReadOnlyCollection<T> AsReadOnly() => new(collection);

        public void ModifyAll(Func<T, T> selector)
        {
            for (var i = 0; i < collection.Count; i++)
                collection[i] = selector(collection[i]);
        }
    }

    public static byte Sum(this IEnumerable<byte> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        byte num1 = 0;
        foreach (var num2 in source)
            checked
            {
                num1 += num2;
            }

        return num1;
    }

    public static sbyte Sum(this IEnumerable<sbyte> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        sbyte num1 = 0;
        foreach (var num2 in source)
            checked
            {
                num1 += num2;
            }

        return num1;
    }

    public static short Sum(this IEnumerable<short> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        short num1 = 0;
        foreach (var num2 in source)
            checked
            {
                num1 += num2;
            }

        return num1;
    }

    public static ushort Sum(this IEnumerable<ushort> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        ushort num1 = 0;
        foreach (var num2 in source)
            checked
            {
                num1 += num2;
            }

        return num1;
    }

    public static uint Sum(this IEnumerable<uint> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        uint num1 = 0;
        foreach (var num2 in source)
            checked
            {
                num1 += num2;
            }

        return num1;
    }

    public static ulong Sum(this IEnumerable<ulong> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        ulong num1 = 0;
        foreach (var num2 in source)
            checked
            {
                num1 += num2;
            }

        return num1;
    }

    public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> enumerable)
        where T : class
    {
        return enumerable
            .Where(static x => x is not null)
            .Select(static x => x!);
    }

    public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> enumerable)
        where T : struct
    {
        return enumerable
            .Where(static x => x.HasValue)
            .Select(static x => x ?? throw new Exception()); // the throw is impossible and is just to shut the IDE up
    }

    public static SortedList<T> ToSortedList<T>(this List<T> list, IComparer<T>? comparer = null) =>
        new(list, comparer!);

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
    {
        return enumerable
            .ToDictionary(
                static pair => pair.Key,
                static pair => pair.Value
            );
    }

    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        where TValue : new() =>
        dict.TryGetValue(key, out var value)
            ? value
            : dict[key] = new TValue();

    extension<TKey, TValue>(IDictionary<TKey, TValue> dict)
    {
        public ReadOnlyDictionary<TKey, TValue> AsReadOnly() => new(dict);

        public void ReplaceWith(IDictionary<TKey, TValue> newDict)
        {
            dict.Clear();
            foreach (var kvp in newDict)
                dict.Add(kvp.Key, kvp.Value);
        }

        public SortedDictionary<TKey, TValue> ToSortedDictionary(IComparer<TKey>? comparer) =>
            new(dict, comparer);
    }

    extension<T>(IEnumerable<T> enumerable)
        where T : struct
    {
        public T? FirstOrNullable()
        {
            if (enumerable is IList<T> { Count: > 0 } list)
                return list[0];

            using var enumerator = enumerable.GetEnumerator();

            return enumerator.MoveNext()
                ? enumerator.Current
                : null;
        }

        public T? FirstOrNullable(Func<T, bool> predicate)
        {
            foreach (var item in enumerable)
                if (predicate(item))
                    return item;

            return null;
        }

        public T? LastOrNullable()
        {
            if (enumerable is IList<T> { Count: > 0 and var count } list)
                return list[count - 1];

            T? result = null;
            foreach (var item in enumerable)
                result = item;

            return result;
        }

        public T? LastOrNullable(Func<T, bool> predicate)
        {
            T? result = null;
            foreach (var item in enumerable)
                if (predicate(item))
                    result = item;

            return result;
        }
    }
}
