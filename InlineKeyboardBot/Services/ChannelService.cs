using InlineKeyboardBot.Data.Repositories.Interfaces;
using InlineKeyboardBot.Models.Dto;
using InlineKeyboardBot.Models.Requests;
using InlineKeyboardBot.Services.Interfaces;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using InlineKeyboardBot.Data.Entities;

namespace InlineKeyboardBot.Services;

public class ChannelService : IChannelService
{
    private readonly IChannelRepository _channelRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<ChannelService> _logger;

    public ChannelService(
        IChannelRepository channelRepository,
        IUserRepository userRepository,
        ITelegramBotClient botClient,
        ILogger<ChannelService> logger)
    {
        _channelRepository = channelRepository;
        _userRepository = userRepository;
        _botClient = botClient;
        _logger = logger;
    }

    public async Task<Channel> CreateChannelAsync(CreateChannelRequest request)
    {
        try
        {
            var channel = new Channel
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Username = request.Username,
                ChatId = request.ChatId,
                Description = request.Description,
                OwnerId = request.OwnerId,
                ClaimStatus = request.OwnerId.HasValue ? ClaimStatus.Claimed : ClaimStatus.Pending,
                ClaimExpiresAt = request.OwnerId.HasValue ? null : DateTime.UtcNow.AddHours(24),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _channelRepository.AddAsync(channel);
            _logger.LogInformation("Channel created: {ChannelName} with status {Status}",
                channel.Title, channel.ClaimStatus);
            return channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating channel {Title}", request.Title);
            throw;
        }
    }

    public async Task<bool> RegisterChannelAsync(Message message)
    {
        try
        {
            if (message.Chat.Type != ChatType.Channel && message.Chat.Type != ChatType.Supergroup)
                return false;
            var existingChannel = await _channelRepository.GetByChatIdAsync(message.Chat.Id);
            if (existingChannel != null)
                return false;
            var request = new CreateChannelRequest
            {
                Title = message.Chat.Title ?? "Unknown Channel",
                Username = message.Chat.Username,
                ChatId = message.Chat.Id,
                Description = message.Chat.Description,
                OwnerId = null
            };
            await CreateChannelAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering channel from message");
            return false;
        }
    }
    public async Task<Channel?> GetChannelByIdAsync(Guid id)
    {
        try
        {
            return await _channelRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel by id {ChannelId}", id);
            return null;
        }
    }

    public async Task<Channel?> GetChannelByChatIdAsync(long chatId)
    {
        try
        {
            return await _channelRepository.GetByChatIdAsync(chatId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel by chat id {ChatId}", chatId);
            return null;
        }
    }

    public async Task<Channel?> GetChannelByUsernameAsync(string username)
    {
        try
        {
            return await _channelRepository.GetByUsernameAsync(username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel by username {Username}", username);
            return null;
        }
    }

    public async Task<ChannelDto?> GetChannelDtoByIdAsync(Guid id)
    {
        try
        {
            var channel = await _channelRepository.GetByIdAsync(id);
            return channel != null ? MapToDto(channel) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel DTO by id {ChannelId}", id);
            return null;
        }
    }

    public async Task<IEnumerable<ChannelDto>> GetUserChannelsAsync(long userId)
    {
        try
        {
            var channels = await _channelRepository.GetChannelsByOwnerIdAsync(userId);
            return channels.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user channels for user {UserId}", userId);
            return Enumerable.Empty<ChannelDto>();
        }
    }

    public async Task<IEnumerable<ChannelDto>> GetAllActiveChannelsAsync()
    {
        try
        {
            var channels = await _channelRepository.GetActiveChannelsAsync();
            return channels.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all active channels");
            return Enumerable.Empty<ChannelDto>();
        }
    }

    public async Task UpdateChannelAsync(Channel channel)
    {
        try
        {
            channel.UpdatedAt = DateTime.UtcNow;
            await _channelRepository.UpdateAsync(channel);
            _logger.LogInformation("Channel updated: {ChannelId}", channel.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating channel {ChannelId}", channel.Id);
            throw;
        }
    }

    public async Task<bool> DeleteChannelAsync(Guid id)
    {
        try
        {
            var channel = await _channelRepository.GetByIdAsync(id);
            if (channel == null)
                return false;

            channel.IsActive = false;
            channel.UpdatedAt = DateTime.UtcNow;
            await _channelRepository.UpdateAsync(channel);

            _logger.LogInformation("Channel deleted: {ChannelId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting channel {ChannelId}", id);
            return false;
        }
    }

    public async Task<bool> IsUserChannelOwnerAsync(long chatId, long userId)
    {
        try
        {
            var channel = await _channelRepository.GetByChatIdAsync(chatId);
            return channel?.OwnerId == userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking channel ownership for user {UserId}, chat {ChatId}",
                userId, chatId);
            return false;
        }
    }

    public async Task<bool> CanSendToChannelAsync(long chatId, long userId)
    {
        try
        {
            // Bot administratormi tekshirish
            var chatMember = await _botClient.GetChatMemberAsync(chatId, _botClient.BotId!.Value);
            if (chatMember.Status != ChatMemberStatus.Administrator)
                return false;

            // User channel egasimi yoki administratorimi tekshirish
            var userChatMember = await _botClient.GetChatMemberAsync(chatId, userId);
            return userChatMember.Status == ChatMemberStatus.Administrator ||
                   userChatMember.Status == ChatMemberStatus.Creator;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking send permissions for user {UserId}, chat {ChatId}",
                userId, chatId);
            return false;
        }
    }

    public async Task UpdateChannelInfoAsync(long chatId)
    {
        try
        {
            var channel = await _channelRepository.GetByChatIdAsync(chatId);
            if (channel == null)
                return;

            var chat = await _botClient.GetChatAsync(chatId);

            channel.Title = chat.Title ?? channel.Title;
            channel.Username = chat.Username;
            channel.Description = chat.Description;
            channel.UpdatedAt = DateTime.UtcNow;

            await _channelRepository.UpdateAsync(channel);
            _logger.LogInformation("Channel info updated: {ChannelId}", channel.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating channel info for chat {ChatId}", chatId);
        }
    }

    private static ChannelDto MapToDto(Channel channel)
    {
        return new ChannelDto
        {
            Id = channel.Id,
            Title = channel.Title,
            Username = channel.Username,
            ChatId = channel.ChatId,
            Description = channel.Description,
            OwnerId = channel.OwnerId,
            IsActive = channel.IsActive,
            CreatedAt = channel.CreatedAt
        };
    }

    public async Task<bool> ClaimChannelByNameAsync(long userId, string channelName)
    {
        try
        {
            var pendingChannels = await _channelRepository.GetPendingChannelsByNameAsync(channelName);

            if (!pendingChannels.Any())
            {
                _logger.LogInformation("No pending channels found with name {ChannelName}", channelName);
                return false;
            }

            if (pendingChannels.Count > 1)
            {
                _logger.LogWarning("Multiple pending channels found with name {ChannelName}. Count: {Count}",
                    channelName, pendingChannels.Count);
                // Birinchisini olish yoki keyinroq selection logic qo'shish mumkin
            }

            var channel = pendingChannels.First();
            channel.OwnerId = userId;
            channel.ClaimStatus = ClaimStatus.Claimed;
            channel.ClaimExpiresAt = null;
            channel.UpdatedAt = DateTime.UtcNow;

            await _channelRepository.UpdateAsync(channel);

            _logger.LogInformation("Channel {ChannelId} claimed by user {UserId}", channel.Id, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming channel {ChannelName} by user {UserId}", channelName, userId);
            return false;
        }
    }
}