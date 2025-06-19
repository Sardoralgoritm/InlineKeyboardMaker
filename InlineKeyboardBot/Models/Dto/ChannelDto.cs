namespace InlineKeyboardBot.Models.Dto;

public class ChannelDto
{
    public Guid Id { get; set; }
    public long ChatId { get; set; }
    public string Title { get; set; } = default!;
    public string? Username { get; set; }
    public string? Description { get; set; }
    public int? MemberCount { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public string? InviteLink { get; set; }
    public DateTime? LastChecked { get; set; }
    public long? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Display properties
    public string DisplayName => !string.IsNullOrEmpty(Username) ? $"{Title} (@{Username})" : Title;
    public string ChannelType => IsPublic ? "Public" : "Private";
    public string MemberCountText => MemberCount?.ToString("N0") ?? "N/A";
}