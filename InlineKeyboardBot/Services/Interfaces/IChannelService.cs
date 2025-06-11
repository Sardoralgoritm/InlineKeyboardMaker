using InlineKeyboardBot.Data.Entities;
using InlineKeyboardBot.Models.Dto;
using InlineKeyboardBot.Models.Requests;
using Telegram.Bot.Types;

namespace InlineKeyboardBot.Services.Interfaces;

public interface IChannelService
{
    Task<Channel> CreateChannelAsync(CreateChannelRequest request);
    Task<bool> RegisterChannelAsync(Message message);
    Task<Channel?> GetChannelByIdAsync(Guid id);
    Task<Channel?> GetChannelByChatIdAsync(long chatId);
    Task<Channel?> GetChannelByUsernameAsync(string username);
    Task<ChannelDto?> GetChannelDtoByIdAsync(Guid id);
    Task<IEnumerable<ChannelDto>> GetUserChannelsAsync(long userId);
    Task<IEnumerable<ChannelDto>> GetAllActiveChannelsAsync();
    Task UpdateChannelAsync(Channel channel);
    Task<bool> DeleteChannelAsync(Guid id);
    Task<bool> IsUserChannelOwnerAsync(long chatId, long userId);
    Task<bool> CanSendToChannelAsync(long chatId, long userId);
    Task UpdateChannelInfoAsync(long chatId);
}