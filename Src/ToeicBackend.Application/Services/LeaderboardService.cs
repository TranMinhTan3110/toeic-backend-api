using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Helpers;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.Application.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly IUserRepository _userRepository;

    public LeaderboardService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetWeeklyLeaderboardAsync(int limit = 50)
    {
        var periodKey = VietnamTimeHelper.GetWeeklyPeriodKey();
        var users = await _userRepository.GetWeeklyLeaderboardAsync(periodKey, limit);

        var rank = 1;
        return users.Select(u => new LeaderboardEntryDto
        {
            Rank = rank++,
            Uid = u.Uid,
            DisplayName = u.DisplayName,
            AvatarUrl = u.AvatarUrl,
            WeeklyEp = u.WeeklyEp,
            StreakDays = u.StreakDays
        }).ToList();
    }
}
