namespace TssUtils.Code.Misc.Extensions;

public static class BasicExtensions
{
    extension(int num)
    {
        public bool IsInRange(int minInclusive, int maxInclusive)
        {
            if (minInclusive > maxInclusive)
                return maxInclusive <= num && num <= minInclusive;
            return minInclusive <= num && num <= maxInclusive;
        }
    }

    extension(float num)
    {
        public bool IsInRange(float minInclusive, float maxInclusive)
        {
            if (minInclusive > maxInclusive)
                return maxInclusive <= num && num <= minInclusive;
            return minInclusive <= num && num <= maxInclusive;
        }
    }

    extension(string str)
    {
        public string Repeat(int count) => string.Concat(Enumerable.Repeat(str, count));

        public string RemoveSuffix(string suffix) =>
            str.EndsWith(suffix)
                ? str[..^suffix.Length]
                : str;

        public string RemovePrefix(string prefix) =>
            str.StartsWith(prefix)
                ? str[prefix.Length..]
                : str;

        public string ToSpaced(bool lowercase = false)
        {
            var result =
                lowercase
                    ? str[0].ToString().ToLower()
                    : str[0].ToString().ToUpper();

            for (var i = 1; i < str.Length; i++)
            {
                var c = str[i];

                if (char.IsLower(c) || (char.IsDigit(c) && char.IsDigit(str[i - 1])))
                {
                    result += c;
                    continue;
                }

                if (!char.IsLetter(c))
                {
                    result += " " + c;
                    continue;
                }


                result += " " + (lowercase ? char.ToLower(c) : char.ToUpper(c));
            }

            return result;
        }

        /// <summary>
        ///     Strips every non-uppercase character from the string
        ///     e.g. <c>GetCurrentName</c> and <c>GetChannelNormalized</c>
        ///     would both produce <c>GCN</c> since these are the only uppercase
        ///     characters in those names
        /// </summary>
        /// <returns>The string without any non-uppercase letters</returns>
        public string OnlyUppercase()
        {
            return str.Where(char.IsUpper).Aggregate(string.Empty, static (current, c) => current + c);
        }
    }

    extension(string? str)
    {
        public string IfHasValueAdd(string addStr, bool before = false)
        {
            addStr = string.IsNullOrWhiteSpace(str)
                ? string.Empty
                : addStr;
            if (before)
                return addStr + str;
            return str + addStr;
        }

        public string ValueOr(string orStr) =>
            (string.IsNullOrWhiteSpace(str)
                ? orStr
                : str)!;
    }
}
