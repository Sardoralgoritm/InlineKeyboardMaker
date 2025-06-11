using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InlineKeyboardBot.Data.Entities;

public class Channel : BaseEntity
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

    public int? MemberCount { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsPublic { get; set; } = false;

    [MaxLength(1000)]
    public string? InviteLink { get; set; }

    public DateTime? LastChecked { get; set; }

    // 🆕 Owner ma'lumotlari
    [Required]
    public long OwnerId { get; set; }

    // Navigation property
    [ForeignKey(nameof(OwnerId))]
    public virtual User Owner { get; set; } = default!;
}