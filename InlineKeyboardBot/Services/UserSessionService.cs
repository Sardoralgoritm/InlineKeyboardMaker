using InlineKeyboardBot.Data.Entities;
using InlineKeyboardBot.Data.Repositories.Interfaces;
using InlineKeyboardBot.Services.Interfaces;
using System.Text.Json;

namespace InlineKeyboardBot.Services;

public class UserSessionService : IUserSessionService
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserSessionService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserSessionService(
        IUserSessionRepository sessionRepository,
        IUserRepository userRepository,
        ILogger<UserSessionService> logger)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<UserSession> CreateSessionAsync(long userId, string state, object? data = null, TimeSpan? expiry = null)
    {
        try
        {
            // Clear existing sessions for this state
            await ClearSessionAsync(userId, state);

            // Get user by TelegramId
            var user = await _userRepository.GetByTelegramIdAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with TelegramId {userId} not found");

            var session = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id, // Guid ID
                State = state,
                Data = data != null ? JsonSerializer.Serialize(data, _jsonOptions) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromHours(2)),
                IsActive = true
            };

            await _sessionRepository.AddAsync(session);

            _logger.LogInformation("Session created: UserId={UserId}, State={State}, ExpiresAt={ExpiresAt}",
                userId, state, session.ExpiresAt);

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user {UserId}, state {State}", userId, state);
            throw;
        }
    }

    public async Task<UserSession?> GetActiveSessionAsync(long userId, string? state = null)
    {
        try
        {
            var session = await _sessionRepository.GetActiveSessionAsync(userId, state);

            if (session != null && IsSessionExpired(session))
            {
                await ClearSessionAsync(userId, session.State);
                _logger.LogDebug("Expired session cleared: UserId={UserId}, State={State}", userId, session.State);
                return null;
            }

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active session for user {UserId}, state {State}", userId, state);
            return null;
        }
    }

    public async Task<T?> GetSessionDataAsync<T>(long userId, string state) where T : class
    {
        try
        {
            var session = await GetActiveSessionAsync(userId, state);

            if (session?.Data == null)
                return null;

            var data = JsonSerializer.Deserialize<T>(session.Data, _jsonOptions);
            _logger.LogDebug("Session data retrieved: UserId={UserId}, State={State}, Type={Type}",
                userId, state, typeof(T).Name);

            return data;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing session data for user {UserId}, state {State}, type {Type}",
                userId, state, typeof(T).Name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session data for user {UserId}, state {State}", userId, state);
            return null;
        }
    }

    public async Task UpdateSessionAsync(UserSession session)
    {
        try
        {
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session);

            _logger.LogDebug("Session updated: UserId={UserId}, State={State}", session.UserId, session.State);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session: UserId={UserId}, State={State}",
                session.UserId, session.State);
            throw;
        }
    }

    public async Task UpdateSessionDataAsync(long userId, string state, object data)
    {
        try
        {
            var session = await GetActiveSessionAsync(userId, state);

            if (session == null)
            {
                _logger.LogWarning("Session not found for update: UserId={UserId}, State={State}", userId, state);
                return;
            }

            session.Data = JsonSerializer.Serialize(data, _jsonOptions);
            session.UpdatedAt = DateTime.UtcNow;

            await _sessionRepository.UpdateAsync(session);

            _logger.LogDebug("Session data updated: UserId={UserId}, State={State}", userId, state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session data: UserId={UserId}, State={State}", userId, state);
            throw;
        }
    }

    public async Task ClearSessionAsync(long userId, string? state = null)
    {
        try
        {
            await _sessionRepository.ClearSessionAsync(userId, state);
            _logger.LogDebug("Session cleared: UserId={UserId}, State={State}", userId, state ?? "all");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing session: UserId={UserId}, State={State}", userId, state);
            throw;
        }
    }

    public async Task ClearExpiredSessionsAsync()
    {
        try
        {
            await _sessionRepository.ClearExpiredSessionsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing expired sessions");
        }
    }

    public async Task<bool> HasActiveSessionAsync(long userId, string state)
    {
        try
        {
            var session = await GetActiveSessionAsync(userId, state);
            return session != null && !IsSessionExpired(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking active session: UserId={UserId}, State={State}", userId, state);
            return false;
        }
    }

    private static bool IsSessionExpired(UserSession session)
    {
        return session.ExpiresAt <= DateTime.UtcNow || !session.IsActive;
    }
}