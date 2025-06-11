using InlineKeyboardBot.Models.Dto.Enums;

namespace InlineKeyboardBot.Models.Dto;

public class PostDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = default!;
    public List<InlineButtonDto> Buttons { get; set; } = new();
    public ButtonLayoutType LayoutType { get; set; }
    public Guid ChannelId { get; set; }
    public string ChannelTitle { get; set; } = default!;
    public long UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public PostStatus Status { get; set; }
    public int? MessageId { get; set; }

    // Statistics
    public int ViewCount { get; set; }
    public int ClickCount { get; set; }

    // Display properties
    public string StatusText => Status switch
    {
        PostStatus.Draft => "Qoralama",
        PostStatus.Sent => "Yuborilgan",
        PostStatus.Failed => "Xatolik",
        _ => "Noma'lum"
    };
}
