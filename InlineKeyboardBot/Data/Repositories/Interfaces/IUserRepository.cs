using InlineKeyboardBot.Data.Entities;

namespace InlineKeyboardBot.Data.Repositories.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByTelegramIdAsync(long telegramId);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<bool> ExistsByTelegramIdAsync(long telegramId);
}