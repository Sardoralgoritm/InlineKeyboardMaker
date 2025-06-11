using InlineKeyboardBot.Models.Dto.Enums;
using InlineKeyboardBot.Models.Dto;
using InlineKeyboardBot.Models.Responses;
using Telegram.Bot.Types.ReplyMarkups;
using InlineKeyboardBot.Models.Requests;

namespace InlineKeyboardBot.Services.Interfaces;

public interface IMessageService
{
    Task<BotResponse> SendMessageAsync(SendMessageRequest request);
    Task<BotResponse> SendMessageToChannelAsync(long chatId, string text, InlineKeyboardMarkup? keyboard = null);
    Task<BotResponse> EditMessageAsync(long chatId, int messageId, string text, InlineKeyboardMarkup? keyboard = null);
    Task<BotResponse> DeleteMessageAsync(long chatId, int messageId);
    Task<BotResponse> AnswerCallbackQueryAsync(string callbackQueryId, string? text = null, bool showAlert = false);
    string FormatPostText(string text, List<InlineButtonDto>? buttons = null);
    InlineKeyboardMarkup CreateInlineKeyboard(List<InlineButtonDto> buttons, ButtonLayoutType layout);
    bool ValidateMessageText(string text);
    bool ValidateUrl(string url);
    string EscapeMarkdown(string text);
}