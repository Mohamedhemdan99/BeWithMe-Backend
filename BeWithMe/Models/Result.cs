namespace BeWithMe.Models
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }

        public static Result Success(string message) =>
            new Result { IsSuccess = true, Message = message, StatusCode = StatusCodes.Status200OK };

        public static Result Failure(string message, int statusCode) =>
            new Result { IsSuccess = false, Message = message, StatusCode = statusCode };
    }
}
