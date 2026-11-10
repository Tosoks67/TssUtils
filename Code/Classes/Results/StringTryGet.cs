namespace TssUtils.Code.Classes.Results;

public readonly struct StringTryGet : ITryGet<string>
{
    private StringTryGet(string str, bool isError = false)
    {
        Success = !isError;
        if (isError)
        {
            Exception = new Exception(str);
            Value = null!;
        }
        else
        {
            Exception = null!;
            Value = str;
        }
    }

    private StringTryGet(Exception exception)
    {
        Success = false;
        Exception = exception;
        Value = null!;
    }

    public static StringTryGet Error(string errorMessage) =>
        new(errorMessage, true);

    public bool Success { get; }

    public Exception Exception => !Success
        ? field
        : throw new InvalidOperationException(
            string.Format(
                Localized.XAccessOnSuccessfulY,
                nameof(Exception),
                nameof(StringTryGet)
            )
        );

    public string Value => Success
        ? field
        : throw new InvalidOperationException(
            string.Format(
                Localized.XAccessOnUnsuccessfulY,
                nameof(Value),
                nameof(StringTryGet)
            )
        );

    public static implicit operator StringTryGet(string value) => new(value);

    public static implicit operator StringTryGet(Result result)
    {
        if (result.Success)
            throw new ArgumentException(
                string.Format(
                    Localized.CannotPassSuccessfulXToYAsZCannotBeRetrieved,
                    nameof(Result),
                    nameof(StringTryGet),
                    nameof(Value)
                ),
                nameof(result)
            );

        return new StringTryGet(result.Exception);
    }

    public static implicit operator StringTryGet(Exception exception) => new(exception);

    public static implicit operator bool(StringTryGet tryGet) => tryGet.Success;

    public static implicit operator Result(StringTryGet tryGet) =>
        tryGet.Success
            ? new Result()
            : new Result(tryGet.Exception);
}
