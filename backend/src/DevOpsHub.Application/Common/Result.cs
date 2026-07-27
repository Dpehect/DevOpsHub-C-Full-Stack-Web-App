namespace DevOpsHub.Application.Common;

public sealed record Error(string Code, string Message);

public class Result
{
    protected Result(bool succeeded, Error? error)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public bool Succeeded { get; }
    public Error? Error { get; }
    public static Result Success() => new(true, null);
    public static Result Failure(string code, string message) => new(false, new Error(code, message));
}

public sealed class Result<T> : Result
{
    private Result(bool succeeded, T? value, Error? error) : base(succeeded, error) => Value = value;
    public T? Value { get; }
    public static Result<T> Success(T value) => new(true, value, null);
    public new static Result<T> Failure(string code, string message) => new(false, default, new Error(code, message));
}
