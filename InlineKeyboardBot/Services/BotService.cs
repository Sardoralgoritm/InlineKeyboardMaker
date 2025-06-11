using InlineKeyboardBot.Configuration;
using InlineKeyboardBot.Services.Interfaces;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot;
using InlineKeyboardBot.Handlers.Interfaces;

namespace InlineKeyboardBot.Services;

public class BotService : IBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUpdateHandler _updateHandler;
    private readonly BotConfiguration _botConfig;
    private readonly ILogger<BotService> _logger;

    public BotService(
        ITelegramBotClient botClient,
        IUpdateHandler updateHandler,
        IOptions<BotConfiguration> botConfig,
        ILogger<BotService> logger)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
        _botConfig = botConfig.Value;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing update {UpdateId} of type {UpdateType}",
                update.Id, update.Type);

            await _updateHandler.HandleUpdateAsync(update, cancellationToken);

            _logger.LogDebug("Successfully processed update {UpdateId}", update.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing update {UpdateId}", update.Id);

            // Critical error'larni handle qilish
            if (ex is ArgumentException or InvalidOperationException)
            {
                _logger.LogWarning("Non-critical error in update {UpdateId}: {Error}", update.Id, ex.Message);
            }
            else
            {
                // Critical error - monitoring system'ga xabar berish mumkin
                _logger.LogCritical(ex, "Critical error processing update {UpdateId}", update.Id);
            }
        }
    }

    public async Task SetWebhookAsync(CancellationToken cancellationToken)
    {
        try
        {
            var webhookUrl = $"{_botConfig.WebhookUrl.TrimEnd('/')}/api/webhook";

            await _botClient.SetWebhookAsync(
                url: webhookUrl,
                secretToken: _botConfig.SecretToken,
                dropPendingUpdates: true, // Eski update'larni o'chirish
                cancellationToken: cancellationToken);

            _logger.LogInformation("Webhook successfully set to: {WebhookUrl}", webhookUrl);

            // Webhook ma'lumotlarini tekshirish
            var webhookInfo = await _botClient.GetWebhookInfoAsync(cancellationToken);
            _logger.LogInformation("Webhook info - URL: {Url}, PendingUpdates: {PendingUpdates}, LastError: {LastError}",
                webhookInfo.Url, webhookInfo.PendingUpdateCount, webhookInfo.LastErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set webhook to {WebhookUrl}", _botConfig.WebhookUrl);
            throw;
        }
    }

    public async Task DeleteWebhookAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _botClient.DeleteWebhookAsync(
                dropPendingUpdates: true,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Webhook successfully deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete webhook");
            throw;
        }
    }

    public async Task<bool> IsWebhookSetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var webhookInfo = await _botClient.GetWebhookInfoAsync(cancellationToken);
            var expectedUrl = $"{_botConfig.WebhookUrl.TrimEnd('/')}/api/webhook";

            var isSet = !string.IsNullOrEmpty(webhookInfo.Url) &&
                       webhookInfo.Url.Equals(expectedUrl, StringComparison.OrdinalIgnoreCase);

            _logger.LogDebug("Webhook check - Expected: {Expected}, Actual: {Actual}, IsSet: {IsSet}",
                expectedUrl, webhookInfo.Url, isSet);

            return isSet;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check webhook status");
            return false;
        }
    }

    /// <summary>
    /// Bot ma'lumotlarini olish
    /// </summary>
    public async Task<User> GetBotInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var botInfo = await _botClient.GetMeAsync(cancellationToken);
            _logger.LogDebug("Bot info - ID: {Id}, Username: {Username}, Name: {Name}",
                botInfo.Id, botInfo.Username, botInfo.FirstName);

            return botInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get bot information");
            throw;
        }
    }

    /// <summary>
    /// Bot buyruqlarini o'rnatish
    /// </summary>
    public async Task SetBotCommandsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var commands = new[]
            {
                new BotCommand { Command = "start", Description = "Botni boshlash" },
                new BotCommand { Command = "help", Description = "Yordam ma'lumotlari" },
                new BotCommand { Command = "newpost", Description = "Yangi post yaratish" },
                new BotCommand { Command = "myposts", Description = "Mening postlarim" },
                new BotCommand { Command = "mychannels", Description = "Mening kanallarim" },
                new BotCommand { Command = "stats", Description = "Statistika" },
                new BotCommand { Command = "settings", Description = "Sozlamalar" },
                new BotCommand { Command = "cancel", Description = "Joriy amalni bekor qilish" }
            };

            await _botClient.SetMyCommandsAsync(commands, cancellationToken: cancellationToken);
            _logger.LogInformation("Bot commands successfully set");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set bot commands");
        }
    }

    /// <summary>
    /// Bot holatini tekshirish
    /// </summary>
    public async Task<bool> CheckBotHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var botInfo = await GetBotInfoAsync(cancellationToken);
            var webhookStatus = await IsWebhookSetAsync(cancellationToken);

            var isHealthy = botInfo != null && webhookStatus;

            _logger.LogInformation("Bot health check - Healthy: {IsHealthy}, Bot: {BotId}, Webhook: {WebhookStatus}",
                isHealthy, botInfo?.Id, webhookStatus);

            return isHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bot health check failed");
            return false;
        }
    }
}