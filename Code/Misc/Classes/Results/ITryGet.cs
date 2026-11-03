namespace TssUtils.Code.Misc.Classes.Results;

public interface ITryGet<out T> : IResult
{
    public T Value { get; }
}

public static class TryGetExtensions
{
    extension<T>(ITryGet<T> tryGet)
    {
        public bool HasErrored(out Exception exception, out T value)
        {
            value = tryGet.Success
                ? tryGet.Value
                : default!;

            return tryGet.HasErrored(out exception);
        }
    }

    extension<T>(ITryGet<T> tryGet)
        where T : class
    {
        public T? ValueOrNull() => tryGet.Success ? tryGet.Value : null;
    }

    extension<T>(ITryGet<T> tryGet)
        where T : struct
    {
        public T? AsNullable() => tryGet.Success ? tryGet.Value : null;
    }
}
