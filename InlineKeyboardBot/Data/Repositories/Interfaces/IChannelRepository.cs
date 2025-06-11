using InlineKeyboardBot.Data.Entities;

namespace InlineKeyboardBot.Data.Repositories.Interfaces;

public interface IChannelRepository : IBaseRepository<Channel>
{
    Task<Channel?> GetByChatIdAsync(long chatId);
    Task<Channel?> GetByUsernameAsync(string username);
    Task<IEnumerable<Channel>> GetActiveChannelsAsync();
    Task<IEnumerable<Channel>> GetPublicChannelsAsync();
    Task<IEnumerable<Channel>> GetChannelsByOwnerIdAsync(long ownerId); // 🆕
    Task<bool> ExistsByChatIdAsync(long chatId);
    Task<bool> IsUserChannelOwnerAsync(long chatId, long userId); // 🆕
}