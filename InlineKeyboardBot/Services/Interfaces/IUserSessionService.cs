using InlineKeyboardBot.Data.Entities;

namespace InlineKeyboardBot.Services.Interfaces;

public interface IUserSessionService
{
    Task<UserSession> CreateSessionAsync(long userId, string state, object? data = null, TimeSpan? expiry = null);
    Task<UserSession?> GetActiveSessionAsync(long userId, string? state = null);
    Task<T?> GetSessionDataAsync<T>(long userId, string state) where T : class;
    Task UpdateSessionAsync(UserSession session);
    Task UpdateSessionDataAsync(long userId, string state, object data);
    Task ClearSessionAsync(long userId, string? state = null);
    Task ClearExpiredSessionsAsync();
    Task<bool> HasActiveSessionAsync(long userId, string state);
}