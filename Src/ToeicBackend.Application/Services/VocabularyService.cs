using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class VocabularyService : IVocabularyService
{
    private readonly IVocabularyRepository _repository;
    private readonly IVocabularyProgressRepository _progressRepository;

    public VocabularyService(IVocabularyRepository repository, IVocabularyProgressRepository progressRepository)
    {
        _repository = repository;
        _progressRepository = progressRepository;
    }

    public async Task<IEnumerable<VocabularyDto>> GetVocabularyListAsync(string? topic, string? level, string? userId = null)
    {
        var entities = await _repository.GetFilteredAsync(topic, level);

        // Nếu có userId → load starred IDs để merge vào kết quả
        HashSet<string>? starredIds = null;
        if (!string.IsNullOrEmpty(userId))
        {
            var allProgress = await _progressRepository.GetUserProgressAsync(userId);
            starredIds = allProgress
                .Where(p => p.IsStarred)
                .Select(p => p.VocabularyId)
                .ToHashSet();
        }

        return entities.Select(e => MapToDto(e, starredIds));
    }

    public async Task<VocabularyDto?> GetVocabularyByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity, null);
    }

    public async Task<IEnumerable<string>> GetTopicsAsync()
    {
        return await _repository.GetTopicsAsync();
    }

    public async Task<IEnumerable<string>> GetLevelsAsync()
    {
        return await _repository.GetLevelsAsync();
    }

    private VocabularyDto MapToDto(Vocabulary entity, HashSet<string>? starredIds)
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
            IsStarred = starredIds?.Contains(entity.Id) ?? false,
            Examples = entity.Examples.Select(e => new VocabularyExampleDto
            {
                Sentence = e.Sentence,
                SentenceVi = e.SentenceVi
            }).ToList()
        };
    }
}
