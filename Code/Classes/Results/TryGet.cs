using TssUtils.Code.Extensions;

namespace TssUtils.Code.Classes.Results;

public readonly struct TryGet<T> : ITryGet<T>
{
    private TryGet(T value)
    {
        Success = true;
        Exception = null!;
        Value = value;
    }

    private TryGet(string errorMessage)
    {
        Success = false;
        Exception = new Exception(errorMessage);
        Value = default!;
    }

    private TryGet(Exception exception)
    {
        Success = false;
        Exception = exception;
        Value = default!;
    }

    public bool Success { get; }

    public Exception Exception => !Success
        ? field
        : throw new InvalidOperationException(
            string.Format(
                Localized.XAccessOnSuccessfulY,
                nameof(Exception),
                nameof(TryGet<>)
            )
        );

    public T Value => Success
        ? field
        : throw new InvalidOperationException(
            string.Format(
                Localized.XAccessOnUnsuccessfulY,
                nameof(Value),
                nameof(TryGet<>)
            )
        );

    public static TryGet<T> RunSafe(Func<T> func)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    public static implicit operator TryGet<T>(T value) => new(value);

    public static implicit operator TryGet<T>(string errorMessage) =>
        new(errorMessage);

    public static implicit operator TryGet<T>(Result result)
    {
        if (result.Success)
            throw new ArgumentException(
                string.Format(
                    Localized.CannotPassSuccessfulXToYAsZCannotBeRetrieved,
                    nameof(Result),
                    nameof(TryGet<>),
                    nameof(Value)
                ),
                nameof(result)
            );

        return new TryGet<T>(result.Exception);
    }

    public static implicit operator TryGet<T>(Exception exception) => new(exception);

    public static implicit operator bool(TryGet<T> tryGet) => tryGet.Success;

    public static implicit operator string(TryGet<T> tryGet) => tryGet.Exception.GetFancyMessage();

    public static implicit operator Result(TryGet<T> tryGet) =>
        tryGet.Success
            ? new Result()
            : new Result(tryGet.Exception);
}
