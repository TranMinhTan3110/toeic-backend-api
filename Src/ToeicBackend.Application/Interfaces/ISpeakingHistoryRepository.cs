using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface ISpeakingHistoryRepository
{
    Task<string> AddAsync(SpeakingHistory history);
    Task<IEnumerable<SpeakingHistory>> GetByUserIdAsync(string userId, string? sessionType = null);
    Task<SpeakingHistory?> GetByIdAsync(string id);
}
