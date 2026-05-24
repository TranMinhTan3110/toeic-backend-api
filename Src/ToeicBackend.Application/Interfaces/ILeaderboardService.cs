using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface ILeaderboardService
{
    Task<IReadOnlyList<LeaderboardEntryDto>> GetWeeklyLeaderboardAsync(int limit = 50);
}
