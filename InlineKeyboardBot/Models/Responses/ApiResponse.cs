namespace InlineKeyboardBot.Models.Responses;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = default!;
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Success(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Success(string message)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Message = message
        };
    }

    public static ApiResponse<T> Error(string message, string? errorCode = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            ErrorCode = errorCode
        };
    }

    public static ApiResponse<T> Error(Exception exception)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = exception.Message,
            ErrorCode = exception.GetType().Name
        };
    }
}