
namespace NYCTaxiData.Application.Common;

public sealed class Result<T>
{ 
    private Result(bool isSuccess, T? value, string? error, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }
     
    public string? ErrorCode { get; }
     
    public static Result<T> Success(T value, string? message = null)
        => new(true, value, message);
     
    public static Result<T> Failure(string error, string? errorCode = null)
        => new(false, default, error, errorCode);
}
