using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface ISpeakingExamHistoryRepository
{
    Task<string> AddAsync(SpeakingExamHistoryDto history);
    Task<IEnumerable<SpeakingExamHistoryDto>> GetByUserIdAsync(string userId);
    Task<SpeakingExamHistoryDto?> GetByIdAsync(string id);
}
