using System.ComponentModel.DataAnnotations;

namespace InlineKeyboardBot.Models.Requests;

public class CreateChannelRequest
{
    [Required]
    public long ChatId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = default!;

    [MaxLength(255)]
    public string? Username { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public long? OwnerId { get; set; }

    public bool IsPublic { get; set; }

    public string? InviteLink { get; set; }
}