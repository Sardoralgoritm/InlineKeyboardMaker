namespace InlineKeyboardBot.Models.Dto;

public class UserDto
{
    public Guid Id { get; set; }
    public long TelegramId { get; set; }
    public string? Username { get; set; }
    public string FirstName { get; set; } = default!;
    public string? LastName { get; set; }
    public string? LanguageCode { get; set; }
    public bool IsBot { get; set; }
    public bool IsPremium { get; set; }
    public DateTime LastActivity { get; set; }
    public DateTime CreatedAt { get; set; }

    // Display properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string DisplayName => !string.IsNullOrEmpty(Username) ? $"@{Username}" : FullName;
}