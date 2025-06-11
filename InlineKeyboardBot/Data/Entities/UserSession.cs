using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InlineKeyboardBot.Data.Entities;

public class UserSession : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string State { get; set; } = default!;

    [Column(TypeName = "jsonb")]
    public string? Data { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = default!;
}