using InlineKeyboardBot.Data.Entities;
using InlineKeyboardBot.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InlineKeyboardBot.Data.Repositories;

public class ChannelRepository : BaseRepository<Channel>, IChannelRepository
{
    public ChannelRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Channel?> GetByChatIdAsync(long chatId)
    {
        return await _dbSet
            .Include(c => c.Owner)
            .FirstOrDefaultAsync(c => c.ChatId == chatId);
    }

    public async Task<Channel?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .Include(c => c.Owner)
            .FirstOrDefaultAsync(c => c.Username == username);
    }

    public async Task<IEnumerable<Channel>> GetActiveChannelsAsync()
    {
        return await _dbSet
            .Include(c => c.Owner)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<Channel>> GetPublicChannelsAsync()
    {
        return await _dbSet
            .Include(c => c.Owner)
            .Where(c => c.IsActive && c.IsPublic)
            .OrderBy(c => c.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<Channel>> GetChannelsByOwnerIdAsync(long ownerId)
    {
        return await _dbSet
            .Include(c => c.Owner)
            .Where(c => c.OwnerId == ownerId && c.IsActive)
            .OrderBy(c => c.Title)
            .ToListAsync();
    }

    public async Task<bool> IsUserChannelOwnerAsync(long chatId, long userId)
    {
        return await _dbSet.AnyAsync(c => c.ChatId == chatId && c.OwnerId == userId);
    }

    public async Task<bool> ExistsByChatIdAsync(long chatId)
    {
        return await _dbSet.AnyAsync(c => c.ChatId == chatId);
    }
}