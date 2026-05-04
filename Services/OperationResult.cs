namespace LibraryBookManagementSystem.Services;

public sealed class OperationResult<T>
{
    private OperationResult(T? value, string? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess => Error is null;

    public static OperationResult<T> Success(T value) => new(value, null);
    public static OperationResult<T> Failure(string error) => new(default, error);
}
