namespace NYCTaxiData.API.Contracts
{
    public sealed class ApiResponse<T>
    {
        public bool IsSuccess { get; init; }
        public T? Data { get; init; }
        public string? Message { get; init; }
        public string? ErrorCode { get; init; }
        public IEnumerable<string>? Errors { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        private ApiResponse() { }

        public static ApiResponse<T> Success(T? data, string? message = null)
            => new()
            {
                IsSuccess = true,
                Data = data,
                Message = message ?? "Operation completed successfully"
            };

        public static ApiResponse<T> Fail(
            string message,
            string? errorCode = null,
            IEnumerable<string>? errors = null)
            => new()
            {
                IsSuccess = false,
                Message = message,
                ErrorCode = errorCode,
                Errors = errors
            };
    }
}
