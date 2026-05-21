using ToeicBackend.Application.DTOs;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IVocabularyProgressService
{
    Task<UpdateProgressResponseDto> UpdateProgressAsync(string userId, UpdateProgressDto dto);
    Task<IEnumerable<UserVocabularyProgress>> GetUserProgressAsync(string userId);
    Task<IEnumerable<string>> GetDueVocabularyIdsAsync(string userId);
    Task<VocabularyHubStatsDto> GetHubStatsAsync(string userId);
    Task<UserVocabularyProgress> ToggleStarAsync(string userId, string vocabularyId);
    Task<IEnumerable<VocabularyDto>> GetStarredVocabulariesAsync(string userId);
    Task<IEnumerable<VocabularyDto>> GetDueVocabulariesAsync(string userId);
    Task<IEnumerable<ReviewScheduleItemDto>> GetReviewScheduleAsync(string userId);
}

