using InlineKeyboardBot.Data.Entities;
using InlineKeyboardBot.Models.Dto;

namespace InlineKeyboardBot.Services.Interfaces;

public interface IUserService
{
    Task<User> GetOrCreateUserAsync(long telegramId, string username, string firstName, string? lastName = null);
    Task<User?> GetUserByTelegramIdAsync(long telegramId);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<UserDto?> GetUserDtoByTelegramIdAsync(long telegramId);
    Task UpdateUserAsync(User user);
    Task UpdateUserActivityAsync(long telegramId);
    Task<bool> DeleteUserAsync(long telegramId);
    Task<IEnumerable<UserDto>> GetActiveUsersAsync();
    Task<int> GetUserCountAsync();
}