namespace InlineKeyboardBot.Models.Responses;

public class WebhookResponse
{
    public bool IsHandled { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public long? UserId { get; set; }
    public string? UpdateType { get; set; }

    public static WebhookResponse Success(long? userId = null, string? updateType = null)
    {
        return new WebhookResponse
        {
            IsHandled = true,
            UserId = userId,
            UpdateType = updateType
        };
    }

    public static WebhookResponse Error(string errorMessage)
    {
        return new WebhookResponse
        {
            IsHandled = false,
            ErrorMessage = errorMessage
        };
    }
}