using InlineKeyboardBot.Data.Entities;

namespace InlineKeyboardBot.Data.Repositories.Interfaces;

public interface IUserSessionRepository : IBaseRepository<UserSession>
{
    Task<UserSession?> GetActiveSessionAsync(long userId, string? state = null);
    Task<List<UserSession>> GetActiveSessionsByUserIdAsync(long userId);
    Task ClearSessionAsync(long userId, string? state = null);
    Task ClearAllSessionsAsync(long userId);
    Task ClearExpiredSessionsAsync();
    Task<UserSession?> GetByUserIdAndStateAsync(long userId, string state);
}
