using System.ComponentModel.DataAnnotations;

namespace InlineKeyboardBot.Data.Entities;

public class User : BaseEntity
{
    [Required]
    public long TelegramId { get; set; }

    [MaxLength(255)]
    public string? Username { get; set; }

    [Required]
    [MaxLength(255)]
    public string FirstName { get; set; } = default!;

    [MaxLength(255)]
    public string? LastName { get; set; }

    [MaxLength(10)]
    public string? LanguageCode { get; set; }

    public bool IsBot { get; set; } = false;

    public bool IsPremium { get; set; } = false;

    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();

    // 🆕 User kanallari
    public virtual ICollection<Channel> OwnedChannels { get; set; } = new List<Channel>();
}