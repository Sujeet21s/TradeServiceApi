namespace TradeServiceApi.Models
{
    public class Response
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public static Response Success(string message, object? data = null)
        {
            return new Response
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static Response Failure(string message)
        {
            return new Response
            {
                IsSuccess = false,
                Message = message
            };
        }
    }
}
