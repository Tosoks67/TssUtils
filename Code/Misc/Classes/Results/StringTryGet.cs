namespace TssUtils.Code.Misc.Classes.Results;

public readonly struct StringTryGet : ITryGet<string>
{
    public StringTryGet(string str, bool isError = false)
    {
        Success = !isError;
        if (isError)
        {
            ErrorMessage = str;
            Value = string.Empty;
        }
        else
        {
            ErrorMessage = string.Empty;
            Value = str;
        }
    }

    public static StringTryGet Error(string errorMessage) =>
        new(errorMessage, true);

    public bool Success { get; }

    public string ErrorMessage => !Success
        ? field
        : throw new InvalidOperationException(
            $"Tried accessing {nameof(ErrorMessage)} on a successful {nameof(StringTryGet)}"
        );

    public string Value => Success
        ? field
        : throw new InvalidOperationException(
            $"Tried accessing {nameof(Value)} on an unsuccessful {nameof(StringTryGet)}"
        );

    public static implicit operator StringTryGet(string value) => new(value);

    public static implicit operator StringTryGet(Result result)
    {
        if (result.Success)
            throw new ArgumentException(
                $"Cannot pass a successful {nameof(Result)} to {nameof(StringTryGet)} " +
                $"as {nameof(Value)} cannot be retrieved from it",
                nameof(result)
            );

        return new StringTryGet(result.ErrorMessage, true);
    }

    public static implicit operator bool(StringTryGet tryGet) => tryGet.Success;

    public static implicit operator Result(StringTryGet tryGet) =>
        tryGet.Success
            ? new Result()
            : new Result(tryGet.ErrorMessage);
}
