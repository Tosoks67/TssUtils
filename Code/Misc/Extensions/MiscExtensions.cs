namespace TssUtils.Code.Misc.Extensions;

public static class MiscExtensions
{
    // ReSharper disable once ReturnTypeCanBeNotNullable
    public static T? Nullable<T>(this T obj) where T : class => obj;

    extension<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
    {
        public void Deconstruct(out TKey key, out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }
    }

    public static string GetFancyMessage(this Exception ex) =>
        $"[{ex.GetType().Format().RemoveSuffix("Exception")}]: {ex.Message}";
}
