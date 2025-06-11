using Telegram.Bot.Types.ReplyMarkups;

namespace InlineKeyboardBot.Models.Responses;

public class BotResponse
{
    public string Text { get; set; } = default!;
    public InlineKeyboardMarkup? ReplyMarkup { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public BotResponseType ResponseType { get; set; } = BotResponseType.Message;

    public static BotResponse Message(string text, InlineKeyboardMarkup? keyboard = null)
    {
        return new BotResponse
        {
            Text = text,
            ReplyMarkup = keyboard,
            ResponseType = BotResponseType.Message
        };
    }

    public static BotResponse EditMessage(string text, InlineKeyboardMarkup? keyboard = null)
    {
        return new BotResponse
        {
            Text = text,
            ReplyMarkup = keyboard,
            ResponseType = BotResponseType.EditMessage
        };
    }

    public static BotResponse AnswerCallback(string text)
    {
        return new BotResponse
        {
            Text = text,
            ResponseType = BotResponseType.AnswerCallback
        };
    }

    public static BotResponse Error(string errorMessage)
    {
        return new BotResponse
        {
            Text = "❌ Xatolik yuz berdi: " + errorMessage,
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ResponseType = BotResponseType.Message
        };
    }
}

public enum BotResponseType
{
    Message,
    EditMessage,
    AnswerCallback,
    DeleteMessage
}