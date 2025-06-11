using InlineKeyboardBot.Handlers.Interfaces;
using InlineKeyboardBot.Models.Telegram;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace InlineKeyboardBot.Handlers;

public class MessageHandler : IMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly IChannelService _channelService;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(
        ITelegramBotClient botClient,
        IUserService userService,
        IChannelService channelService,
        ILogger<MessageHandler> logger)
    {
        _botClient = botClient;
        _userService = userService;
        _channelService = channelService;
        _logger = logger;
    }

    public bool CanHandle(Message message)
    {
        // Text message'larni handle qilamiz
        return message.Type == MessageType.Text &&
               !string.IsNullOrEmpty(message.Text) &&
               !message.Text.StartsWith("/");
    }

    public async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling text message from user {UserId}: {MessageText}",
                message.From?.Id, message.Text);

            // User ma'lumotlarini yangilash
            await UpdateUserActivity(message);

            // Message turini aniqlash va handle qilish
            if (IsButtonFormat(message.Text!))
            {
                await HandleButtonInput(message, cancellationToken);
            }
            else if (IsUrlInput(message.Text!))
            {
                await HandleUrlInput(message, cancellationToken);
            }
            else
            {
                await HandleTextInput(message, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling message from user {UserId}", message.From?.Id);
            await SendErrorMessage(message.Chat.Id, "Xabarni qayta ishlashda xatolik yuz berdi.");
        }
    }

    private async Task UpdateUserActivity(Message message)
    {
        if (message.From != null)
        {
            try
            {
                await _userService.GetOrCreateUserAsync(
                    message.From.Id,
                    message.From.Username ?? "",
                    message.From.FirstName,
                    message.From.LastName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user activity for {UserId}", message.From.Id);
            }
        }
    }

    private bool IsButtonFormat(string text)
    {
        // Tugma formatini tekshirish: "Tugma nomi | https://example.com"
        return text.Contains(" | ") && text.Split(" | ").Length == 2;
    }

    private bool IsUrlInput(string text)
    {
        // URL formatini tekshirish
        return text.StartsWith("http://") || text.StartsWith("https://") || text.StartsWith("t.me/");
    }

    private async Task HandleButtonInput(Message message, CancellationToken cancellationToken)
    {
        var parts = message.Text!.Split(" | ");
        var buttonText = parts[0].Trim();
        var buttonUrl = parts[1].Trim();

        // URL validatsiyasi
        if (!IsValidUrl(buttonUrl))
        {
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "❌ Noto'g'ri URL format!\n\n" +
                      "To'g'ri format: `Tugma nomi | https://example.com`\n\n" +
                      "Misol: `Sotib olish | https://myshop.com`",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
            return;
        }

        // Tugma nomi validatsiyasi  
        if (string.IsNullOrWhiteSpace(buttonText) || buttonText.Length > 64)
        {
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "❌ Tugma nomi 1-64 belgi orasida bo'lishi kerak!",
                cancellationToken: cancellationToken);
            return;
        }

        // Tugmani session ga saqlash (keyinroq implement qilamiz)
        // await SaveButtonToSession(message.From!.Id, buttonText, buttonUrl);

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("➕ Yana tugma qo'shish", CallbackCommands.AddButtons),
                InlineKeyboardButton.WithCallbackData("✅ Tugmalarni tugatish", CallbackCommands.FinishButtons)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("❌ Bekor qilish", CallbackCommands.CancelPost)
            }
        });

        await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"✅ Tugma qo'shildi!\n\n" +
                  $"📝 **Tugma:** {buttonText}\n" +
                  $"🔗 **Link:** {buttonUrl}\n\n" +
                  "Yana tugma qo'shasizmi?",
            parseMode: ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleUrlInput(Message message, CancellationToken cancellationToken)
    {
        var url = message.Text!.Trim();

        if (!IsValidUrl(url))
        {
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "❌ Noto'g'ri URL format!\n\n" +
                      "To'g'ri formatlar:\n" +
                      "• https://example.com\n" +
                      "• http://example.com\n" +
                      "• t.me/username",
                cancellationToken: cancellationToken);
            return;
        }

        await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"🔗 URL qabul qilindi: {url}\n\n" +
                  "Tugma nomi qanday bo'lsin?\n\n" +
                  "Yoki to'liq formatda yuboring: `Tugma nomi | {url}`",
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }

    private async Task HandleTextInput(Message message, CancellationToken cancellationToken)
    {
        var text = message.Text!.Trim();

        // Post text validation
        if (text.Length > 4096)
        {
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "❌ Post matni 4096 belgidan oshmasligi kerak!\n\n" +
                      $"Hozirgi uzunlik: {text.Length} belgi",
                cancellationToken: cancellationToken);
            return;
        }

        if (text.Length < 1)
        {
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "❌ Post matni bo'sh bo'lishi mumkin emas!",
                cancellationToken: cancellationToken);
            return;
        }

        // Post textini session ga saqlash (keyinroq implement qilamiz)
        // await SavePostTextToSession(message.From!.Id, text);

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔘 Tugma qo'shish", CallbackCommands.AddButtons),
                InlineKeyboardButton.WithCallbackData("⏭ Tugmasiz davom etish", CallbackCommands.SkipButtons)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("❌ Bekor qilish", CallbackCommands.CancelPost)
            }
        });

        await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"✅ Post matni saqlandi!\n\n" +
                  $"📝 **Matn uzunligi:** {text.Length} belgi\n\n" +
                  "Endi tugma qo'shasizmi?",
            parseMode: ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private bool IsValidUrl(string url)
    {
        try
        {
            // Basic URL validation
            if (url.StartsWith("t.me/"))
            {
                return url.Length > 5; // t.me/ dan keyin biror narsa bo'lishi kerak
            }

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
        catch
        {
            return false;
        }
    }

    private async Task SendErrorMessage(long chatId, string errorMessage)
    {
        try
        {
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"❌ {errorMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send error message to chat {ChatId}", chatId);
        }
    }
}