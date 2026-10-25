namespace TssUtils.Code.Misc.Classes.Results;

public readonly struct Result : IResult
{
    public Result()
    {
        Success = true;
        ErrorMessage = string.Empty;
    }

    public Result(string errorMessage)
    {
        Success = false;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }

    public string ErrorMessage => !Success
        ? field
        : throw new InvalidOperationException(
            $"Tried accessing {nameof(ErrorMessage)} on a successful {nameof(Result)}"
        );

    public static Result RunSafe(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception e)
        {
            return e.ToString();
        }
    }

    public static implicit operator Result(bool success)
    {
        if (!success)
            throw new InvalidOperationException(
                $"{nameof(Result)} cannot be false without an error message"
            );

        return new Result();
    }

    public static implicit operator Result(string errorMessage) =>
        new(errorMessage);

    public static implicit operator bool(Result result) => result.Success;

    public static explicit operator string(Result result) => result.ErrorMessage;
}
