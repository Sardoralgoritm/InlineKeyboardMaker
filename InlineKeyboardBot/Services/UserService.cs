using InlineKeyboardBot.Data.Entities;
using InlineKeyboardBot.Data.Repositories.Interfaces;
using InlineKeyboardBot.Models.Dto;
using InlineKeyboardBot.Services.Interfaces;

namespace InlineKeyboardBot.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User> GetOrCreateUserAsync(long telegramId, string username, string firstName, string? lastName = null)
    {
        try
        {
            var existingUser = await _userRepository.GetByTelegramIdAsync(telegramId);

            if (existingUser != null)
            {
                // Mavjud user ma'lumotlarini yangilash
                var wasUpdated = false;

                if (existingUser.Username != username)
                {
                    existingUser.Username = username;
                    wasUpdated = true;
                }

                if (existingUser.FirstName != firstName)
                {
                    existingUser.FirstName = firstName;
                    wasUpdated = true;
                }

                if (existingUser.LastName != lastName)
                {
                    existingUser.LastName = lastName;
                    wasUpdated = true;
                }

                existingUser.LastActivity = DateTime.UtcNow;

                if (wasUpdated)
                {
                    await _userRepository.UpdateAsync(existingUser);
                    _logger.LogDebug("Updated user {TelegramId} information", telegramId);
                }
                else
                {
                    await UpdateUserActivityAsync(telegramId);
                }

                return existingUser;
            }

            // Yangi user yaratish
            var newUser = new User
            {
                TelegramId = telegramId,
                Username = username,
                FirstName = firstName,
                LastName = lastName,
                LastActivity = DateTime.UtcNow
            };

            var createdUser = await _userRepository.AddAsync(newUser);
            _logger.LogInformation("Created new user {TelegramId} - {FirstName} {LastName}",
                telegramId, firstName, lastName);

            return createdUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or creating user {TelegramId}", telegramId);
            throw;
        }
    }

    public async Task<User?> GetUserByTelegramIdAsync(long telegramId)
    {
        try
        {
            return await _userRepository.GetByTelegramIdAsync(telegramId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by Telegram ID {TelegramId}", telegramId);
            throw;
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        try
        {
            return await _userRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID {UserId}", id);
            throw;
        }
    }

    public async Task<UserDto?> GetUserDtoByTelegramIdAsync(long telegramId)
    {
        try
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);

            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                TelegramId = user.TelegramId,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                LanguageCode = user.LanguageCode,
                IsBot = user.IsBot,
                IsPremium = user.IsPremium,
                LastActivity = user.LastActivity,
                CreatedAt = user.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user DTO by Telegram ID {TelegramId}", telegramId);
            throw;
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        try
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await _userRepository.UpdateAsync(user);
            _logger.LogDebug("Updated user {TelegramId}", user.TelegramId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {TelegramId}", user?.TelegramId);
            throw;
        }
    }

    public async Task UpdateUserActivityAsync(long telegramId)
    {
        try
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);

            if (user == null)
            {
                _logger.LogWarning("Attempted to update activity for non-existent user {TelegramId}", telegramId);
                return;
            }

            user.LastActivity = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            _logger.LogDebug("Updated activity for user {TelegramId}", telegramId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user activity {TelegramId}", telegramId);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(long telegramId)
    {
        try
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);

            if (user == null)
            {
                _logger.LogWarning("Attempted to delete non-existent user {TelegramId}", telegramId);
                return false;
            }

            await _userRepository.DeleteAsync(user.Id);
            _logger.LogInformation("Deleted user {TelegramId}", telegramId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {TelegramId}", telegramId);
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetActiveUsersAsync()
    {
        try
        {
            var users = await _userRepository.GetActiveUsersAsync();

            return users.Select(user => new UserDto
            {
                Id = user.Id,
                TelegramId = user.TelegramId,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                LanguageCode = user.LanguageCode,
                IsBot = user.IsBot,
                IsPremium = user.IsPremium,
                LastActivity = user.LastActivity,
                CreatedAt = user.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active users");
            throw;
        }
    }

    public async Task<int> GetUserCountAsync()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            return users.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user count");
            throw;
        }
    }

    public async Task<Dictionary<string, object>> GetUserStatsAsync(long telegramId)
    {
        try
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);

            if (user == null)
                throw new ArgumentException("User not found", nameof(telegramId));

            var stats = new Dictionary<string, object>
            {
                { "user_id", user.TelegramId },
                { "username", user.Username ?? "N/A" },
                { "full_name", $"{user.FirstName} {user.LastName}".Trim() },
                { "registration_date", user.CreatedAt },
                { "last_activity", user.LastActivity },
                { "is_premium", user.IsPremium },
                { "days_since_registration", (DateTime.UtcNow - user.CreatedAt).Days },
                { "days_since_last_activity", (DateTime.UtcNow - user.LastActivity).Days }
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user stats for {TelegramId}", telegramId);
            throw;
        }
    }

    public bool ValidateUserData(string firstName, string? lastName = null, string? username = null)
    {
        // FirstName majburiy
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length > 255)
            return false;

        // LastName ixtiyoriy, lekin agar berilgan bo'lsa validatsiya
        if (!string.IsNullOrEmpty(lastName) && lastName.Length > 255)
            return false;

        // Username ixtiyoriy, lekin agar berilgan bo'lsa format tekshirish
        if (!string.IsNullOrEmpty(username))
        {
            if (username.Length > 255 || !username.All(c => char.IsLetterOrDigit(c) || c == '_'))
                return false;
        }

        return true;
    }
}