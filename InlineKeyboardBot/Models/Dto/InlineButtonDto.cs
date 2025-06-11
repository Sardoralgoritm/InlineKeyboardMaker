namespace InlineKeyboardBot.Models.Dto;

public class InlineButtonDto
{
    public string Text { get; set; } = default!;
    public string Url { get; set; } = default!;
    public int Order { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }

    // Statistics
    public int ClickCount { get; set; }
}
