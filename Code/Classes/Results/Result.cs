using TssUtils.Code.Extensions;

namespace TssUtils.Code.Classes.Results;

public readonly struct Result : IResult
{
    public Result()
    {
        Success = true;
        Exception = null!;
    }

    public Result(string errorMessage)
    {
        Success = false;
        Exception = new Exception(errorMessage);
    }

    public Result(Exception exception)
    {
        Success = false;
        Exception = exception;
    }

    public bool Success { get; }

    public Exception Exception => !Success
        ? field
        : throw new InvalidOperationException(
            $"Tried accessing {nameof(Exception)} on a successful {nameof(Result)}"
        );

    public static Result RunSafe(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception ex)
        {
            return ex.ToString();
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

    public static implicit operator Result(Exception exception) =>
        new(exception);

    public static implicit operator bool(Result result) => result.Success;

    public static explicit operator string(Result result) => result.Exception.GetFancyMessage();
}
