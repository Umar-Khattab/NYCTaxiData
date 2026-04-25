using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Common.Plumping
{
    public class Result
    {
        protected Result(bool isSuccess, string? message = null, string? errorCode = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            ErrorCode = errorCode;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Message { get; }
        public string? ErrorCode { get; }

        // ===== Factory Methods =====
        public static Result Success(string? message = null)
            => new(true, message);

        public static Result Failure(string message, string? errorCode = null)
            => new(false, message, errorCode);

        public static Result<T> Success<T>(T data, string? message = null)
            => Result<T>.Success(data, message);

        public static Result<T> Failure<T>(string message, string? errorCode = null)
            => Result<T>.Failure(message, errorCode);
    }

    public class Result<T> : Result
    {
        private Result(
            bool isSuccess,
            T? data,
            string? message = null,
            string? errorCode = null)
            : base(isSuccess, message, errorCode)
        {
            Data = data;
        }

        public T? Data { get; }

        // ===== Factory Methods =====
        public static Result<T> Success(T data, string? message = null)
            => new(true, data, message);

        public new static Result<T> Failure(string message, string? errorCode = null)
            => new(false, default, message, errorCode);

        // ===== Implicit Conversion =====
        public static implicit operator Result<T>(T data)
            => Success(data);
    }
}
