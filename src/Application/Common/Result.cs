using System.Collections.Generic;
using System.Linq;

namespace ProductManagement.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Message { get; }
    public int StatusCode { get; }
    public IEnumerable<string> Errors { get; }

    protected Result(bool isSuccess, string message, int statusCode, IEnumerable<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        StatusCode = statusCode;
        Errors = errors ?? Enumerable.Empty<string>();
    }

    public static Result Success(string message = "", int statusCode = 200) =>
        new Result(true, message, statusCode);

    public static Result Failure(string message, int statusCode = 400, IEnumerable<string>? errors = null) =>
        new Result(false, message, statusCode, errors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string message, int statusCode, IEnumerable<string>? errors = null)
        : base(isSuccess, message, statusCode, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value, string message = "", int statusCode = 200) =>
        new Result<T>(true, value, message, statusCode);

    public static new Result<T> Failure(string message, int statusCode = 400, IEnumerable<string>? errors = null) =>
        new Result<T>(false, default, message, statusCode, errors);
}

