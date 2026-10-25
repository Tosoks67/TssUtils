using TssUtils.Code.Misc.Extensions;

namespace TssUtils.Code.Misc.Classes.Results;

public readonly struct TryGet<T> : ITryGet<T>
{
    public TryGet(T value)
    {
        Success = true;
        ErrorMessage = string.Empty;
        Value = value;
    }

    public TryGet(string errorMessage)
    {
        Success = false;
        ErrorMessage = errorMessage;
        Value = default!;
    }

    public bool Success { get; }

    public string ErrorMessage => !Success
        ? field
        : throw new InvalidOperationException(
            $"Tried accessing {nameof(ErrorMessage)} on a successful {nameof(TryGet<>)}"
        );

    public T Value => Success
        ? field
        : throw new InvalidOperationException(
            $"Tried accessing {nameof(Value)} on an unsuccessful {nameof(TryGet<>)}"
        );

    public static TryGet<T> RunSafe(Func<T> func, bool fullErrorMessage = false)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            return fullErrorMessage
                ? ex.ToString()
                : ex.GetFancyMessage();
        }
    }

    public static implicit operator TryGet<T>(T value) => new(value);

    public static implicit operator TryGet<T>(string errorMessage) =>
        new(errorMessage);

    public static implicit operator TryGet<T>(Result result)
    {
        if (result.Success)
            throw new ArgumentException(
                $"Cannot pass a successful {nameof(Result)} to {nameof(TryGet<>)} " +
                $"as {nameof(Value)} cannot be retrieved from it",
                nameof(result)
            );

        return new TryGet<T>(result.ErrorMessage);
    }

    public static implicit operator bool(TryGet<T> tryGet) => tryGet.Success;

    public static implicit operator string(TryGet<T> tryGet) => tryGet.ErrorMessage;

    public static implicit operator Result(TryGet<T> tryGet) =>
        tryGet.Success
            ? new Result()
            : new Result(tryGet.ErrorMessage);
}
