namespace TssUtils.Code.Misc.Classes.Results;

public interface IResult
{
    public bool Success { get; }
    public Exception Exception { get; }
}

public static class ResultExtensions
{
    extension(IResult result)
    {
        public bool HasErrored(out Exception exception)
        {
            exception = result.Success
                ? null!
                : result.Exception;

            return !result.Success;
        }
    }
}
