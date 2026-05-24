using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IVocabularyRepository
{
    Task<IEnumerable<Vocabulary>> GetFilteredAsync(string? topic, string? level);
    Task<Vocabulary?> GetByIdAsync(string id);
    Task<IEnumerable<string>> GetTopicsAsync();
    Task<IEnumerable<string>> GetLevelsAsync();
    Task<int> GetCountAsync();
    Task<IEnumerable<Vocabulary>> GetByIdsAsync(IEnumerable<string> ids);
}

