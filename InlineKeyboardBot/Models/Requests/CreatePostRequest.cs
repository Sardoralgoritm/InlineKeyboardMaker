using InlineKeyboardBot.Models.Dto.Enums;
using InlineKeyboardBot.Models.Dto;
using System.ComponentModel.DataAnnotations;

namespace InlineKeyboardBot.Models.Requests;

public class CreatePostRequest
{
    [Required]
    [MaxLength(4096)]
    public string Content { get; set; } = default!;

    public List<InlineButtonDto> Buttons { get; set; } = new();

    public ButtonLayoutType LayoutType { get; set; } = ButtonLayoutType.SingleColumn;

    [Required]
    public Guid ChannelId { get; set; }

    [Required]
    public long UserId { get; set; }

    public DateTime? ScheduledAt { get; set; }
}