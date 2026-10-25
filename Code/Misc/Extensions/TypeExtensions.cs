using System.Collections.Immutable;
using System.Reflection;

namespace TssUtils.Code.Misc.Extensions;

public static class TypeExtensions
{
    private static readonly ImmutableHashSet<Type> InheritanceChainSkippedClasses =
        ImmutableHashSet.Create(
            typeof(object),
            typeof(ValueType),
            typeof(Enum)
        );

    extension(Type type)
    {
        public bool IsIntegerType => Type.GetTypeCode(type) switch
        {
            TypeCode.Byte or
                TypeCode.SByte or
                TypeCode.Int16 or
                TypeCode.UInt16 or
                TypeCode.Int32 or
                TypeCode.UInt32 or
                TypeCode.Int64 or
                TypeCode.UInt64 => true,
            _ => false
        };

        public bool IsFloatingPointType => Type.GetTypeCode(type) switch
        {
            TypeCode.Single or
                TypeCode.Double or
                TypeCode.Decimal => true,
            _ => false
        };

        public bool IsUnsignedIntegerType => Type.GetTypeCode(type) switch
        {
            TypeCode.Byte or
                TypeCode.UInt16 or
                TypeCode.UInt32 or
                TypeCode.UInt64 => true,
            _ => false
        };

        public string Format()
        {
            var result = type.Name;

            if (!type.IsGenericType)
                return result;

            var tick = result.IndexOf('`');

            if (tick >= 0)
                result = result[..tick];

            return $"{result}<" +
                   type.GetGenericArguments()
                       .Select(static arg =>
                           arg.Format()
                       )
                       .JoinStrings() +
                   ">";
        }

        /// <summary>
        ///     Gets an array containing every base type of <paramref name="type" />
        ///     where the first element is <paramref name="type" /> itself and the last
        ///     is the farthest non-skipped parent class type
        /// </summary>
        /// <remarks>
        ///     This method skips base types such as
        ///     <see cref="object" /> or <see cref="ValueType" />.
        ///     See <see cref="TypeExtensions.InheritanceChainSkippedClasses" />
        ///     for every skipped class
        /// </remarks>
        /// <returns>
        ///     Array of every base type of <paramref name="type" />
        ///     including <paramref name="type" /> itself
        /// </returns>
        public Type[] GetInheritanceChain()
        {
            return [.. GetChain()];

            IEnumerable<Type> GetChain()
            {
                for (var current = type;
                     current is not null;
                     current = current.BaseType)
                    if (!InheritanceChainSkippedClasses.Contains(current))
                        yield return current;
            }
        }
    }

    extension(PropertyInfo property)
    {
        public bool IsStatic => (property.GetMethod ?? property.SetMethod)?.IsStatic is true;
    }
}
