using InlineKeyboardBot.Models.Dto.Enums;
using InlineKeyboardBot.Models.Dto;
using System.ComponentModel.DataAnnotations;

namespace InlineKeyboardBot.Models.Requests;

public class SendMessageRequest
{
    [Required]
    public long ChatId { get; set; }

    [Required]
    [MaxLength(4096)]
    public string Text { get; set; } = default!;

    public List<InlineButtonDto> Buttons { get; set; } = new();

    public ButtonLayoutType LayoutType { get; set; } = ButtonLayoutType.SingleColumn;

    public bool DisableWebPagePreview { get; set; } = false;

    public bool DisableNotification { get; set; } = false;

    public int? ReplyToMessageId { get; set; }
}