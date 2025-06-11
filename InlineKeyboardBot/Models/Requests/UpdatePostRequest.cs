using InlineKeyboardBot.Models.Dto.Enums;
using InlineKeyboardBot.Models.Dto;
using System.ComponentModel.DataAnnotations;

namespace InlineKeyboardBot.Models.Requests;

public class UpdatePostRequest
{
    [Required]
    public Guid PostId { get; set; }

    [MaxLength(4096)]
    public string? Content { get; set; }

    public List<InlineButtonDto>? Buttons { get; set; }

    public ButtonLayoutType? LayoutType { get; set; }

    public Guid? ChannelId { get; set; }
}