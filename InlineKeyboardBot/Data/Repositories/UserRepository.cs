using InlineKeyboardBot.Data.Entities;
using InlineKeyboardBot.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InlineKeyboardBot.Data.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByTelegramIdAsync(long telegramId)
    {
        return await _dbSet
            .Include(u => u.Sessions)
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .Include(u => u.Sessions)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbSet
            .Where(u => u.LastActivity >= DateTime.UtcNow.AddDays(-30))
            .OrderByDescending(u => u.LastActivity)
            .ToListAsync();
    }

    public async Task<bool> ExistsByTelegramIdAsync(long telegramId)
    {
        return await _dbSet.AnyAsync(u => u.TelegramId == telegramId);
    }
}