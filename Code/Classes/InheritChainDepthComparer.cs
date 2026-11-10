using System.Collections.Concurrent;
using TssUtils.Code.Extensions;

namespace TssUtils.Code.Classes;

public class InheritChainDepthComparer(Type mustInheritFrom) : IComparer<Type>
{
    private static readonly ConcurrentDictionary<Type, int> ChainDepthCache = new();

    private Type MustInheritFrom { get; } = mustInheritFrom;

    public int Compare(Type? x, Type? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return 1;
        if (y is null) return -1;

        return GetDepth(y).CompareTo(GetDepth(x));
    }

    private int GetDepth(Type t)
    {
        if (!MustInheritFrom.IsAssignableFrom(t))
            throw new InvalidCastException(
                string.Format(
                    Localized.InheritChainNotDerivedException,
                    t.Format(),
                    MustInheritFrom.Format()
                )
            );

        return ChainDepthCache.GetOrAdd(
            t,
            static t => t.GetInheritanceChain().Length
        );
    }
}
