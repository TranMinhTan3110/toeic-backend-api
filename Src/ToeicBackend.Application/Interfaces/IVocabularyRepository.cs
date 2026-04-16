using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IVocabularyRepository
{
    Task<IEnumerable<Vocabulary>> GetAllAsync();
    Task<IEnumerable<Vocabulary>> GetByTopicAsync(string topic);
    Task<IEnumerable<Vocabulary>> GetByTopicAndLevelAsync(string topic, string level);
    Task<Vocabulary?> GetByIdAsync(string id);
}
