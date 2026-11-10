using System.ComponentModel;
using System.Reflection;
using TssUtils.Code.Classes;

namespace TssUtils.Code.Extensions;

public static class EnumExtensions
{
    extension<T>(T value) where T : struct, Enum
    {
        public ushort Weight => typeof(T)
            .GetField(value.ToString())?
            .GetCustomAttribute<EnumWeightAttribute>()?
            .Weight ?? 0;

        public static IEnumerable<T> GetValues() => Enum.GetValues(typeof(T)).Cast<T>();

        public IEnumerable<T> GetFlags()
        {
            if (value.Equals(default(T)))
                return [];

            return GetValues<T>()
                .Where(static flag => !flag.Equals(default(T)))
                .Where(flag => value.HasFlag(flag));
        }
    }

    extension(Enum value)
    {
        public DescriptionAttribute? DescriptionAttribute => value
            .GetType()
            .GetField(value.ToString())?
            .GetCustomAttribute<DescriptionAttribute>();

        public string Description => value
                                         .DescriptionAttribute?
                                         .Description
                                     ?? string.Empty;

        public string DescriptionOrValue => value.Description.IsEmpty()
            ? value.ToString()
            : value.Description;

        public long AsLong() =>
            Enum.GetUnderlyingType(value.GetType()) == typeof(ulong)
                ? unchecked((long)Convert.ToUInt64(value))
                : Convert.ToInt64(value);
    }
}
