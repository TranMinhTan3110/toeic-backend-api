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

    public async Task<VocabularyDto> CreateVocabularyAsync(CreateVocabularyDto dto)
    {
        var entity = new Vocabulary
        {
            Id = Guid.NewGuid().ToString(),
            Word = dto.Word.Trim(),
            Phonetic = dto.Phonetic?.Trim(),
            WordType = dto.WordType.Trim().ToLowerInvariant(),
            DefinitionEn = dto.DefinitionEn.Trim(),
            DefinitionVi = dto.DefinitionVi.Trim(),
            AudioUrl = dto.AudioUrl?.Trim(),
            ImageUrl = dto.ImageUrl?.Trim(),
            Topic = dto.Topic.Trim().ToLowerInvariant(),
            Level = dto.Level.Trim(),
            Frequency = dto.Frequency.Trim().ToLowerInvariant(),
            Synonyms = dto.Synonyms.Select(s => s.Trim()).ToList(),
            Antonyms = dto.Antonyms.Select(a => a.Trim()).ToList(),
            Collocations = dto.Collocations.Select(c => c.Trim()).ToList(),
            Examples = dto.Examples.Select(e => new VocabularyExample
            {
                Sentence = e.Sentence.Trim(),
                SentenceVi = e.SentenceVi.Trim()
            }).ToList(),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(entity);
        return MapToDto(entity, null);
    }

    public async Task<VocabularyDto?> UpdateVocabularyAsync(string id, CreateVocabularyDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        existing.Word = dto.Word.Trim();
        existing.Phonetic = dto.Phonetic?.Trim();
        existing.WordType = dto.WordType.Trim().ToLowerInvariant();
        existing.DefinitionEn = dto.DefinitionEn.Trim();
        existing.DefinitionVi = dto.DefinitionVi.Trim();
        existing.AudioUrl = dto.AudioUrl?.Trim();
        existing.ImageUrl = dto.ImageUrl?.Trim();
        existing.Topic = dto.Topic.Trim().ToLowerInvariant();
        existing.Level = dto.Level.Trim();
        existing.Frequency = dto.Frequency.Trim().ToLowerInvariant();
        existing.Synonyms = dto.Synonyms.Select(s => s.Trim()).ToList();
        existing.Antonyms = dto.Antonyms.Select(a => a.Trim()).ToList();
        existing.Collocations = dto.Collocations.Select(c => c.Trim()).ToList();
        existing.Examples = dto.Examples.Select(e => new VocabularyExample
        {
            Sentence = e.Sentence.Trim(),
            SentenceVi = e.SentenceVi.Trim()
        }).ToList();

        await _repository.UpdateAsync(existing);
        return MapToDto(existing, null);
    }

    public async Task<bool> DeleteVocabularyAsync(string id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task<int> BulkCreateVocabularyAsync(List<CreateVocabularyDto> dtos)
    {
        if (dtos == null || !dtos.Any()) return 0;

        var entities = dtos.Select(dto => new Vocabulary
        {
            Id = Guid.NewGuid().ToString(),
            Word = dto.Word.Trim(),
            Phonetic = dto.Phonetic?.Trim(),
            WordType = dto.WordType.Trim().ToLowerInvariant(),
            DefinitionEn = dto.DefinitionEn.Trim(),
            DefinitionVi = dto.DefinitionVi.Trim(),
            AudioUrl = dto.AudioUrl?.Trim(),
            ImageUrl = dto.ImageUrl?.Trim(),
            Topic = dto.Topic.Trim().ToLowerInvariant(),
            Level = dto.Level.Trim(),
            Frequency = dto.Frequency.Trim().ToLowerInvariant(),
            Synonyms = dto.Synonyms.Select(s => s.Trim()).ToList(),
            Antonyms = dto.Antonyms.Select(a => a.Trim()).ToList(),
            Collocations = dto.Collocations.Select(c => c.Trim()).ToList(),
            Examples = dto.Examples.Select(e => new VocabularyExample
            {
                Sentence = e.Sentence.Trim(),
                SentenceVi = e.SentenceVi.Trim()
            }).ToList(),
            CreatedAt = DateTime.UtcNow
        }).ToList();

        return await _repository.BulkCreateAsync(entities);
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
