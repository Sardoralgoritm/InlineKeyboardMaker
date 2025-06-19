using InlineKeyboardBot.Data.Entities;
using InlineKeyboardBot.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InlineKeyboardBot.Data.Repositories;

public class UserSessionRepository : BaseRepository<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<UserSession?> GetActiveSessionAsync(long userId, string? state = null)
    {
        try
        {
            var query = _dbSet.Where(s => s.User.TelegramId == userId &&
                                         s.IsActive &&
                                         s.ExpiresAt > DateTime.UtcNow);

            if (!string.IsNullOrEmpty(state))
            {
                query = query.Where(s => s.State == state);
            }

            return await query.OrderByDescending(s => s.CreatedAt)
                            .FirstOrDefaultAsync();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<List<UserSession>> GetActiveSessionsByUserIdAsync(long userId)
    {
        try
        {
            return await _dbSet.Where(s => s.User.TelegramId == userId &&
                                         s.IsActive &&
                                         s.ExpiresAt > DateTime.UtcNow)
                              .ToListAsync();
        }
        catch (Exception)
        {
            return new List<UserSession>();
        }
    }

    public async Task ClearSessionAsync(long userId, string? state = null)
    {
        try
        {
            var query = _dbSet.Where(s => s.User.TelegramId == userId && s.IsActive);

            if (!string.IsNullOrEmpty(state))
            {
                query = query.Where(s => s.State == state);
            }

            var sessions = await query.ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
                session.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task ClearAllSessionsAsync(long userId)
    {
        try
        {
            var sessions = await _dbSet.Where(s => s.User.TelegramId == userId && s.IsActive)
                                     .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
                session.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task ClearExpiredSessionsAsync()
    {
        try
        {
            var expiredSessions = await _dbSet.Where(s => s.IsActive && s.ExpiresAt <= DateTime.UtcNow)
                                            .ToListAsync();

            foreach (var session in expiredSessions)
            {
                session.IsActive = false;
                session.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<UserSession?> GetByUserIdAndStateAsync(long userId, string state)
    {
        try
        {
            return await _dbSet.Where(s => s.User.TelegramId == userId &&
                                         s.State == state &&
                                         s.IsActive &&
                                         s.ExpiresAt > DateTime.UtcNow)
                              .FirstOrDefaultAsync();
        }
        catch (Exception)
        {
            return null;
        }
    }
}