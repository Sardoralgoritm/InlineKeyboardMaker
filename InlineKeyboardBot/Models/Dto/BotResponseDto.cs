namespace InlineKeyboardBot.Models.Dto;

public class BotResponseDto
{
    public string Message { get; set; } = default!;
    public bool IsSuccess { get; set; }
    public string? ErrorCode { get; set; }
    public object? Data { get; set; }
    public List<InlineButtonDto>? Buttons { get; set; }

    public static BotResponseDto Success(string message, object? data = null)
    {
        return new BotResponseDto
        {
            Message = message,
            IsSuccess = true,
            Data = data
        };
    }

    public static BotResponseDto Error(string message, string? errorCode = null)
    {
        return new BotResponseDto
        {
            Message = message,
            IsSuccess = false,
            ErrorCode = errorCode
        };
    }
}