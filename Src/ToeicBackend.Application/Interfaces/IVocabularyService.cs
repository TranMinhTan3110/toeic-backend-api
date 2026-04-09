using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IVocabularyService
{
    Task<IEnumerable<VocabularyDto>> GetAllVocabularyAsync();
    Task<IEnumerable<VocabularyDto>> GetVocabularyByTopicAsync(string topic);
    Task<IEnumerable<VocabularyDto>> GetVocabularyByTopicAndLevelAsync(string topic, string level);
    Task<VocabularyDto?> GetVocabularyByIdAsync(string id);
}
