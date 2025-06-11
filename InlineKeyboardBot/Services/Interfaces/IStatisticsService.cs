using InlineKeyboardBot.Models.Responses;

namespace InlineKeyboardBot.Services.Interfaces;

public interface IStatisticsService
{
    Task<ChannelStatsResponse> GetChannelStatsAsync(Guid channelId, long userId);
    Task<List<PostStatsDto>> GetTopPostsAsync(Guid? channelId = null, int count = 10);
    Task<List<ButtonStatsDto>> GetTopButtonsAsync(Guid? channelId = null, int count = 10);
    Task<Dictionary<string, int>> GetDailyStatsAsync(DateTime date);
    Task<Dictionary<string, int>> GetWeeklyStatsAsync(DateTime startDate);
    Task<Dictionary<string, int>> GetMonthlyStatsAsync(int year, int month);
    Task TrackButtonClickAsync(Guid postId, string buttonUrl, long? userId = null);
    Task TrackPostViewAsync(Guid postId, long? userId = null);
}