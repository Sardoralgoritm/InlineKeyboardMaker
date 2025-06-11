using InlineKeyboardBot.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InlineKeyboardBot.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.TelegramId).IsUnique();
            entity.HasIndex(e => e.Username);
            entity.Property(e => e.TelegramId).IsRequired();
            entity.Property(e => e.FirstName).IsRequired();
        });

        // Channel entity configuration
        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasIndex(e => e.ChatId).IsUnique();
            entity.HasIndex(e => e.Username);
            entity.HasIndex(e => e.OwnerId); // 🆕 Owner index
            entity.Property(e => e.ChatId).IsRequired();
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.OwnerId).IsRequired();

            // 🆕 Foreign key relationship
            entity.HasOne(e => e.Owner)
                  .WithMany(u => u.OwnedChannels)
                  .HasForeignKey(e => e.OwnerId)
                  .HasPrincipalKey(u => u.TelegramId) // TelegramId ga bog'lash
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // UserSession entity configuration
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.State });
            entity.HasIndex(e => e.ExpiresAt);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.State).IsRequired();

            // Foreign key relationship
            entity.HasOne(d => d.User)
                  .WithMany(p => p.Sessions)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Global query filters for soft delete
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Channel>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserSession>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (
                e.State == EntityState.Added ||
                e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}