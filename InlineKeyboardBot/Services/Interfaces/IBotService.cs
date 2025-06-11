using Telegram.Bot.Types;

namespace InlineKeyboardBot.Services.Interfaces;

public interface IBotService
{
    Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
    Task SetWebhookAsync(CancellationToken cancellationToken);
    Task DeleteWebhookAsync(CancellationToken cancellationToken);
    Task<bool> IsWebhookSetAsync(CancellationToken cancellationToken);
}