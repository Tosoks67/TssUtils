namespace TssUtils.Code.Misc.Classes.Results;

public interface IResult
{
    public bool Success { get; }
    public string ErrorMessage { get; }
}

public static class ResultExtensions
{
    extension(IResult result)
    {
        public bool HasErrored(out string errorMessage)
        {
            errorMessage = result.Success
                ? null!
                : result.ErrorMessage;

            return !result.Success;
        }

        public bool HasErrored() =>
            result.HasErrored(out _);

        public void ThrowIfErrored(Func<string, Exception>? errorFunc = null)
        {
            errorFunc ??= static message => new Exception(message);

            if (!result.Success)
                throw errorFunc(result.ErrorMessage);
        }
    }
}
