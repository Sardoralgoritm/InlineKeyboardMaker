namespace InlineKeyboardBot.Models.Responses;

public class ChannelStatsResponse
{
    public Guid ChannelId { get; set; }
    public string ChannelTitle { get; set; } = default!;
    public int TotalPosts { get; set; }
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public double ClickThroughRate { get; set; }
    public DateTime? LastPostDate { get; set; }
    public List<PostStatsDto> RecentPosts { get; set; } = new();
    public List<ButtonStatsDto> PopularButtons { get; set; } = new();
}

public class PostStatsDto
{
    public Guid PostId { get; set; }
    public string Content { get; set; } = default!;
    public DateTime SentAt { get; set; }
    public int ViewCount { get; set; }
    public int ClickCount { get; set; }
    public double ClickThroughRate { get; set; }
}

public class ButtonStatsDto
{
    public string ButtonText { get; set; } = default!;
    public string ButtonUrl { get; set; } = default!;
    public int ClickCount { get; set; }
    public int PostCount { get; set; } // Nechta postda ishlatilgan
}