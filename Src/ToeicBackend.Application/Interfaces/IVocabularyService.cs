using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IVocabularyService
{
    Task<IEnumerable<VocabularyDto>> GetVocabularyListAsync(string? topic, string? level, string? userId = null);
    Task<VocabularyDto?> GetVocabularyByIdAsync(string id);
    Task<IEnumerable<string>> GetTopicsAsync();
    Task<IEnumerable<string>> GetLevelsAsync();
    Task<VocabularyDto> CreateVocabularyAsync(CreateVocabularyDto dto);
    Task<VocabularyDto?> UpdateVocabularyAsync(string id, CreateVocabularyDto dto);
    Task<bool> DeleteVocabularyAsync(string id);
    Task<int> BulkCreateVocabularyAsync(List<CreateVocabularyDto> dtos);
}
