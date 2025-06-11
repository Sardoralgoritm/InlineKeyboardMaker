namespace InlineKeyboardBot.Configuration;

public class BotConfiguration
{
    public string BotToken { get; set; } = default!;
    public string WebhookUrl { get; set; } = default!;
    public string SecretToken { get; set; } = default!;
    public string[] AllowedUpdates { get; set; } = default!;
}