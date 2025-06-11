using InlineKeyboardBot.Models.Dto;
using InlineKeyboardBot.Models.Responses;
using InlineKeyboardBot.Services.Interfaces;
using System.Text.RegularExpressions;
using System.Text;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using InlineKeyboardBot.Models.Requests;
using InlineKeyboardBot.Models.Dto.Enums;

namespace InlineKeyboardBot.Services;

public class MessageService : IMessageService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<MessageService> _logger;

    // URL validation regex
    private readonly Regex _urlRegex = new(@"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public MessageService(
        ITelegramBotClient botClient,
        ILogger<MessageService> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task<BotResponse> SendMessageAsync(SendMessageRequest request)
    {
        try
        {
            if (!ValidateMessageText(request.Text))
            {
                return BotResponse.Error("Xabar matni noto'g'ri formatda");
            }

            // Inline keyboard yaratish
            InlineKeyboardMarkup? keyboard = null;
            if (request.Buttons.Any())
            {
                keyboard = CreateInlineKeyboard(request.Buttons, request.LayoutType);
            }

            var message = await _botClient.SendTextMessageAsync(
                chatId: request.ChatId,
                text: request.Text,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard,
                disableWebPagePreview: request.DisableWebPagePreview,
                disableNotification: request.DisableNotification,
                replyToMessageId: request.ReplyToMessageId);

            _logger.LogInformation("Message sent to chat {ChatId}, message id: {MessageId}",
                request.ChatId, message.MessageId);

            return BotResponse.Message($"Xabar yuborildi (ID: {message.MessageId})", keyboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to chat {ChatId}", request.ChatId);
            return BotResponse.Error($"Xabar yuborishda xatolik: {ex.Message}");
        }
    }

    public async Task<BotResponse> SendMessageToChannelAsync(long chatId, string text, InlineKeyboardMarkup? keyboard = null)
    {
        try
        {
            if (!ValidateMessageText(text))
            {
                return BotResponse.Error("Xabar matni noto'g'ri formatda");
            }

            var message = await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard,
                disableWebPagePreview: false);

            _logger.LogInformation("Message sent to channel {ChatId}, message id: {MessageId}",
                chatId, message.MessageId);

            return BotResponse.Message($"Xabar kanalga yuborildi (ID: {message.MessageId})", keyboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to channel {ChatId}", chatId);
            return BotResponse.Error($"Kanalga xabar yuborishda xatolik: {ex.Message}");
        }
    }

    public async Task<BotResponse> EditMessageAsync(long chatId, int messageId, string text, InlineKeyboardMarkup? keyboard = null)
    {
        try
        {
            if (!ValidateMessageText(text))
            {
                return BotResponse.Error("Xabar matni noto'g'ri formatda");
            }

            await _botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: messageId,
                text: text,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard);

            _logger.LogInformation("Message edited in chat {ChatId}, message id: {MessageId}",
                chatId, messageId);

            return BotResponse.EditMessage("Xabar tahrirlandi", keyboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing message {MessageId} in chat {ChatId}", messageId, chatId);
            return BotResponse.Error($"Xabarni tahrirlashda xatolik: {ex.Message}");
        }
    }

    public async Task<BotResponse> DeleteMessageAsync(long chatId, int messageId)
    {
        try
        {
            await _botClient.DeleteMessageAsync(chatId, messageId);

            _logger.LogInformation("Message deleted from chat {ChatId}, message id: {MessageId}",
                chatId, messageId);

            return new BotResponse
            {
                Text = "Xabar o'chirildi",
                ResponseType = BotResponseType.DeleteMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId} from chat {ChatId}", messageId, chatId);
            return BotResponse.Error($"Xabarni o'chirishda xatolik: {ex.Message}");
        }
    }

    public async Task<BotResponse> AnswerCallbackQueryAsync(string callbackQueryId, string? text = null, bool showAlert = false)
    {
        try
        {
            await _botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQueryId,
                text: text,
                showAlert: showAlert);

            _logger.LogInformation("Callback query answered: {CallbackQueryId}", callbackQueryId);

            return BotResponse.AnswerCallback(text ?? "✅ Amal bajarildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error answering callback query {CallbackQueryId}", callbackQueryId);
            return BotResponse.Error($"Callback javob berishda xatolik: {ex.Message}");
        }
    }

    public string FormatPostText(string text, List<InlineButtonDto>? buttons = null)
    {
        var formattedText = new StringBuilder();

        // Asosiy matnni qo'shamiz
        formattedText.AppendLine(text);

        // Agar tugmalar bo'lsa, ularni matn ostiga qo'shamiz (ixtiyoriy)
        if (buttons != null && buttons.Any())
        {
            formattedText.AppendLine();
            formattedText.AppendLine("📌 <i>Qo'shimcha havolalar:</i>");

            foreach (var button in buttons.Where(b => !string.IsNullOrEmpty(b.Url)))
            {
                formattedText.AppendLine($"• <a href=\"{button.Url}\">{button.Text}</a>");
            }
        }

        return formattedText.ToString().Trim();
    }

    public InlineKeyboardMarkup CreateInlineKeyboard(List<InlineButtonDto> buttons, ButtonLayoutType layout)
    {
        if (!buttons.Any())
            return new InlineKeyboardMarkup(Array.Empty<InlineKeyboardButton[]>());

        var keyboardButtons = new List<List<InlineKeyboardButton>>();

        switch (layout)
        {
            case ButtonLayoutType.SingleColumn:
                // Har bir tugma alohida qatorda
                foreach (var button in buttons)
                {
                    keyboardButtons.Add(new List<InlineKeyboardButton> { CreateButton(button) });
                }
                break;

            case ButtonLayoutType.TwoColumns:
                // Har qatorda 2 ta tugma
                for (int i = 0; i < buttons.Count; i += 2)
                {
                    var row = new List<InlineKeyboardButton>();
                    row.Add(CreateButton(buttons[i]));

                    if (i + 1 < buttons.Count)
                    {
                        row.Add(CreateButton(buttons[i + 1]));
                    }

                    keyboardButtons.Add(row);
                }
                break;

            case ButtonLayoutType.ThreeColumns:
                // Har qatorda 3 ta tugma
                for (int i = 0; i < buttons.Count; i += 3)
                {
                    var row = new List<InlineKeyboardButton>();
                    row.Add(CreateButton(buttons[i]));

                    if (i + 1 < buttons.Count)
                    {
                        row.Add(CreateButton(buttons[i + 1]));
                    }

                    if (i + 2 < buttons.Count)
                    {
                        row.Add(CreateButton(buttons[i + 2]));
                    }

                    keyboardButtons.Add(row);
                }
                break;

            case ButtonLayoutType.AllInOneRow:
                // Barcha tugmalar bitta qatorda
                var singleRow = buttons.Select(CreateButton).ToList();
                keyboardButtons.Add(singleRow);
                break;

            case ButtonLayoutType.Custom:
            default:
                // Custom yoki default: avtomatik tartib
                var currentRow = new List<InlineKeyboardButton>();
                int currentRowLength = 0;
                const int maxRowLength = 40; // Maksimal qator uzunligi

                foreach (var button in buttons)
                {
                    var buttonLength = button.Text.Length;

                    if (currentRowLength + buttonLength > maxRowLength && currentRow.Any())
                    {
                        keyboardButtons.Add(new List<InlineKeyboardButton>(currentRow));
                        currentRow.Clear();
                        currentRowLength = 0;
                    }

                    currentRow.Add(CreateButton(button));
                    currentRowLength += buttonLength;
                }

                if (currentRow.Any())
                {
                    keyboardButtons.Add(currentRow);
                }
                break;
        }

        return new InlineKeyboardMarkup(keyboardButtons.Select(row => row.ToArray()));
    }

    public bool ValidateMessageText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        // Telegram xabar uzunligi chegarasi
        if (text.Length > 4096)
            return false;

        // HTML teglarini tekshirish (oddiy tekshiruv)
        var openTags = Regex.Matches(text, @"<(\w+)>").Count;
        var closeTags = Regex.Matches(text, @"</(\w+)>").Count;

        // Har bir ochiq teg uchun yopiq teg bo'lishi kerak
        return openTags == closeTags;
    }

    public bool ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return _urlRegex.IsMatch(url);
    }

    public string EscapeMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // HTML entities
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;");
    }

    private InlineKeyboardButton CreateButton(InlineButtonDto button)
    {
        if (!string.IsNullOrEmpty(button.Url) && ValidateUrl(button.Url))
        {
            return InlineKeyboardButton.WithUrl(button.Text, button.Url);
        }
        else
        {
            // Default: callback data sifatida button text'ini ishlatamiz
            return InlineKeyboardButton.WithCallbackData(button.Text, button.Text.ToLower().Replace(" ", "_"));
        }
    }
}