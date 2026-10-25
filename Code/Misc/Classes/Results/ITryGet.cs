namespace TssUtils.Code.Misc.Classes.Results;

public interface ITryGet<out T> : IResult
{
    public T Value { get; }
}

public static class TryGetExtensions
{
    extension<T>(ITryGet<T> tryGet)
    {
        public bool HasErrored(out string errorMessage, out T value)
        {
            value = tryGet.Success
                ? tryGet.Value
                : default!;

            return tryGet.HasErrored(out errorMessage);
        }

        public void ThrowIfErrored(
            out T value,
            Func<string, Exception>? errorFunc = null)
        {
            tryGet.ThrowIfErrored(errorFunc);
            value = tryGet.Value;
        }
    }
}
