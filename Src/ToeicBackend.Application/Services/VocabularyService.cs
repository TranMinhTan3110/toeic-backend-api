using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class VocabularyService : IVocabularyService
{
    private readonly IVocabularyRepository _repository;

    public VocabularyService(IVocabularyRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<VocabularyDto>> GetAllVocabularyAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<VocabularyDto>> GetVocabularyByTopicAsync(string topic)
    {
        var entities = await _repository.GetByTopicAsync(topic);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<VocabularyDto>> GetVocabularyByTopicAndLevelAsync(string topic, string level)
    {
        var entities = await _repository.GetByTopicAndLevelAsync(topic, level);
        return entities.Select(MapToDto);
    }

    public async Task<VocabularyDto?> GetVocabularyByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    private VocabularyDto MapToDto(Vocabulary entity)
    {
        return new VocabularyDto
        {
            Id = entity.Id,
            Word = entity.Word,
            Phonetic = entity.Phonetic,
            WordType = entity.WordType,
            DefinitionEn = entity.DefinitionEn,
            DefinitionVi = entity.DefinitionVi,
            AudioUrl = entity.AudioUrl,
            ImageUrl = entity.ImageUrl,
            Topic = entity.Topic,
            Level = entity.Level,
            Frequency = entity.Frequency,
            Synonyms = entity.Synonyms,
            Antonyms = entity.Antonyms,
            Collocations = entity.Collocations,
            Examples = entity.Examples.Select(e => new VocabularyExampleDto
            {
                Sentence = e.Sentence,
                SentenceVi = e.SentenceVi
            }).ToList()
        };
    }
}
