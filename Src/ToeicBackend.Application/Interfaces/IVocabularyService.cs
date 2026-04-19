using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IVocabularyService
{
    Task<IEnumerable<VocabularyDto>> GetVocabularyListAsync(string? topic, string? level);
    Task<VocabularyDto?> GetVocabularyByIdAsync(string id);
    Task<IEnumerable<string>> GetTopicsAsync();
    Task<IEnumerable<string>> GetLevelsAsync();
}
