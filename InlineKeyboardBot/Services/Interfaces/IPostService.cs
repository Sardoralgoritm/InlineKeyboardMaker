using InlineKeyboardBot.Models.Dto;
using InlineKeyboardBot.Models.Requests;
using InlineKeyboardBot.Models.Responses;

namespace InlineKeyboardBot.Services.Interfaces;

public interface IPostService
{
    Task<PostDto> CreatePostAsync(CreatePostRequest request);
    Task<PostDto?> GetPostByIdAsync(Guid id);
    Task<IEnumerable<PostDto>> GetUserPostsAsync(long userId, int page = 1, int pageSize = 10);
    Task<IEnumerable<PostDto>> GetChannelPostsAsync(Guid channelId, int page = 1, int pageSize = 10);
    Task<PostDto> UpdatePostAsync(UpdatePostRequest request);
    Task<bool> DeletePostAsync(Guid id, long userId);
    Task<bool> SendPostToChannelAsync(Guid postId, long userId);
    Task<PostStatsDto> GetPostStatsAsync(Guid postId);
    Task IncrementPostViewAsync(Guid postId);
    Task IncrementButtonClickAsync(Guid postId, string buttonUrl);
}
