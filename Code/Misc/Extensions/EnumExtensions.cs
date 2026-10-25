using System.ComponentModel;
using System.Reflection;
using TssUtils.Code.Misc.Classes;
using TssUtils.Code.Misc.Classes.Results;
using UnityEngine;

namespace TssUtils.Code.Misc.Extensions;

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

        public static T? GetRandom()
        {
            var values = GetValues<T>().ToArray();

            var areAllZero = values.All(static value => value.Weight == 0);

            return values
                .GetRandomWeighted(value =>
                    areAllZero ? (ushort)1 : value.Weight
                )
                .HasErrored(
                    out _,
                    out var chosenValue
                )
                ? null
                : chosenValue;
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

    extension(Color color)
    {
        public static Color FromHex(uint hex, bool withAlpha = false)
        {
            if (withAlpha)
                return new Color(
                    ((hex >> 24) & 0xFF) / 255f,
                    ((hex >> 16) & 0xFF) / 255f,
                    ((hex >> 8) & 0xFF) / 255f,
                    (hex & 0xFF) / 255f
                );
            return new Color(
                ((hex >> 16) & 0xFF) / 255f,
                ((hex >> 8) & 0xFF) / 255f,
                (hex & 0xFF) / 255f
            );
        }

        public string ToHex(bool withAlpha = false) => ((Color32)color).ToHex(withAlpha);
    }

    extension(Color32 color)
    {
        public string ToHex(bool withAlpha = false) =>
            withAlpha
                ? $"#{color.r:X2}{color.g:X2}{color.b:X2}{color.a:X2}"
                : $"#{color.r:X2}{color.g:X2}{color.b:X2}";
    }
}
