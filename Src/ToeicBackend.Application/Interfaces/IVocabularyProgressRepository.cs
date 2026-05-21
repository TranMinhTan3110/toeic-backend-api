using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IVocabularyProgressRepository
{
    Task<UserVocabularyProgress?> GetAsync(string userId, string vocabularyId);
    Task UpsertAsync(UserVocabularyProgress progress);
    Task<IEnumerable<UserVocabularyProgress>> GetUserProgressAsync(string userId);
    Task<IEnumerable<string>> GetDueVocabularyIdsAsync(string userId);
}
