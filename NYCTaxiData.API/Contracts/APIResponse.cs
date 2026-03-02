namespace NYCTaxiData.API.Contracts
{
    public class APIResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public object? Error { get; set; }

    }
}
