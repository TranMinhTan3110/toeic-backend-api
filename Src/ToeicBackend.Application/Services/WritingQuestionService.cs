using Microsoft.Extensions.Caching.Memory;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class WritingQuestionService : IWritingQuestionService
{
    private readonly IWritingQuestionRepository _repository;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan CountCacheDuration = TimeSpan.FromHours(1);

    private const string AllCacheKey = "writing_questions_all";
    private const string PracticeCacheKey = "writing_questions_practice";
    private const string TypesCacheKey = "writing_questions_types";
    private const string CountsCacheKey = "writing_questions_practice_counts";
    private static string DetailCacheKey(string id) => $"writing_questions_detail_{id}";
    private static string TypeCacheKey(string taskType) => $"writing_questions_type_{Normalize(taskType)}";
    private static string PracticeTypeCacheKey(string taskType) => $"writing_questions_practice_type_{Normalize(taskType)}";
    private static string ExamTypeCacheKey(string taskType) => $"writing_questions_exam_type_{Normalize(taskType)}";
    private static string NumberCacheKey(int taskNumber) => $"writing_questions_number_{taskNumber}";
    private static string DifficultyCacheKey(string difficulty) => $"writing_questions_difficulty_{Normalize(difficulty)}";
    private static string ExamSetCacheKey(string examSetId) => $"writing_questions_exam_set_{examSetId}";

    public WritingQuestionService(IWritingQuestionRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetAllAsync()
    {
        if (_cache.TryGetValue(AllCacheKey, out List<WritingQuestionDto>? cached) && cached != null)
            return cached;

        var entities = await _repository.GetAllAsync();
        var result = entities.Select(MapToDto).ToList();
        _cache.Set(AllCacheKey, result, CacheDuration);
        return result;
    }

    public async Task<WritingQuestionDto?> GetByIdAsync(string id)
    {
        var cacheKey = DetailCacheKey(id);
        if (_cache.TryGetValue(cacheKey, out WritingQuestionDto? cached) && cached != null)
            return cached;

        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        var result = MapToDto(entity);
        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetByTaskTypeAsync(string taskType)
    {
        var cacheKey = TypeCacheKey(taskType);
        if (_cache.TryGetValue(cacheKey, out List<WritingQuestionDto>? cached) && cached != null)
            return cached;

        var entities = await _repository.GetByTaskTypeAsync(taskType);
        var result = entities.Select(MapToDto).ToList();
        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetByTaskNumberAsync(int taskNumber)
    {
        var cacheKey = NumberCacheKey(taskNumber);
        if (_cache.TryGetValue(cacheKey, out List<WritingQuestionDto>? cached) && cached != null)
            return cached;

        var entities = await _repository.GetByTaskNumberAsync(taskNumber);
        var result = entities.Select(MapToDto).ToList();
        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetByDifficultyAsync(string difficulty)
    {
        var cacheKey = DifficultyCacheKey(difficulty);
        if (_cache.TryGetValue(cacheKey, out List<WritingQuestionDto>? cached) && cached != null)
            return cached;

        var entities = await _repository.GetByDifficultyAsync(difficulty);
        var result = entities.Select(MapToDto).ToList();
        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetPracticeQuestionsAsync()
    {
        if (_cache.TryGetValue(PracticeCacheKey, out List<WritingQuestionDto>? cached) && cached != null)
            return cached;

        var entities = await _repository.GetPracticeQuestionsAsync();
        var result = entities.Select(MapToDto).ToList();
        _cache.Set(PracticeCacheKey, result, CacheDuration);
        return result;
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetPracticeByTaskTypeAsync(string taskType)
    {
        var cacheKey = PracticeTypeCacheKey(taskType);
        if (_cache.TryGetValue(cacheKey, out List<WritingQuestionDto>? cached) && cached != null)
            return cached;

        var entities = await _repository.GetPracticeByTaskTypeAsync(taskType);
        var result = entities.Select(MapToDto).ToList();
        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetExamByTaskTypeAsync(string taskType)
    {
        var cacheKey = ExamTypeCacheKey(taskType);
        if (_cache.TryGetValue(cacheKey, out List<WritingQuestionDto>? cached) && cached != null)
            return cached;

        var entities = await _repository.GetExamByTaskTypeAsync(taskType);
        var result = entities.Select(MapToDto).ToList();
        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<IEnumerable<string>> GetAvailableTaskTypesAsync()
    {
        if (_cache.TryGetValue(TypesCacheKey, out List<string>? cached) && cached != null)
            return cached;

        var result = (await _repository.GetAvailableTaskTypesAsync()).ToList();
        _cache.Set(TypesCacheKey, result, CountCacheDuration);
        return result;
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetQuestionsByExamSetIdAsync(string examSetId)
    {
        var cacheKey = ExamSetCacheKey(examSetId);
        if (_cache.TryGetValue(cacheKey, out List<WritingQuestionDto>? cached) && cached != null)
            return cached;

        var entities = await _repository.GetQuestionsByExamSetIdAsync(examSetId);
        var result = entities.Select(MapToDto).ToList();
        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<Dictionary<string, int>> GetPracticeCountsByTaskTypeAsync()
    {
        if (_cache.TryGetValue(CountsCacheKey, out Dictionary<string, int>? cached) && cached != null)
            return cached;

        var result = await _repository.GetPracticeCountsByTaskTypeAsync();
        _cache.Set(CountsCacheKey, result, CountCacheDuration);
        return result;
    }

    public async Task<string> AddAsync(WritingQuestionDto dto)
    {
        var entity = MapToEntity(dto);
        if (string.IsNullOrWhiteSpace(entity.Id))
            entity.Id = "admin_w_q_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var id = await _repository.AddAsync(entity);
        entity.Id = id;
        ClearListCaches(entity);
        _cache.Set(DetailCacheKey(id), MapToDto(entity), CacheDuration);
        return id;
    }

    public async Task<bool> UpdateAsync(string id, WritingQuestionDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        var entity = MapToEntity(dto);
        entity.Id = id;
        var result = await _repository.UpdateAsync(id, entity);
        if (result)
        {
            ClearListCaches(existing);
            ClearListCaches(entity);
            _cache.Remove(DetailCacheKey(id));
        }
        return result;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        var result = await _repository.DeleteAsync(id);
        if (result)
        {
            ClearListCaches(existing);
            _cache.Remove(DetailCacheKey(id));
        }
        return result;
    }

    private static string Normalize(string? value) => value?.Trim().ToLowerInvariant() ?? string.Empty;

    private void ClearListCaches(WritingQuestion question)
    {
        _cache.Remove(AllCacheKey);
        _cache.Remove(PracticeCacheKey);
        _cache.Remove(TypesCacheKey);
        _cache.Remove(CountsCacheKey);
        _cache.Remove(TypeCacheKey(question.TaskType));
        _cache.Remove(PracticeTypeCacheKey(question.TaskType));
        _cache.Remove(ExamTypeCacheKey(question.TaskType));
        _cache.Remove(NumberCacheKey(question.TaskNumber));
        _cache.Remove(DifficultyCacheKey(question.Difficulty));
        if (!string.IsNullOrWhiteSpace(question.ExamSetId))
            _cache.Remove(ExamSetCacheKey(question.ExamSetId));
    }

    private WritingQuestion MapToEntity(WritingQuestionDto dto)
    {
        return new WritingQuestion
        {
            Id = dto.Id,
            TaskNumber = dto.TaskNumber,
            TaskType = Normalize(dto.TaskType),
            PromptText = dto.PromptText ?? string.Empty,
            PromptImageUrl = dto.PromptImageUrl,
            GivenWords = dto.GivenWords ?? new(),
            EmailContent = dto.EmailContent,
            EmailQuestions = dto.EmailQuestions ?? new(),
            TimeLimit = dto.TimeLimit,
            MinWords = dto.MinWords,
            MaxWords = dto.MaxWords,
            MaxScore = dto.MaxScore,
            ScoringCriteria = dto.ScoringCriteria ?? new(),
            SampleAnswer = dto.SampleAnswer,
            SampleAnswerTranslation = dto.SampleAnswerTranslation,
            ExplanationVietnamese = dto.ExplanationVietnamese,
            Topic = dto.Topic,
            Difficulty = string.IsNullOrWhiteSpace(dto.Difficulty) ? "medium" : Normalize(dto.Difficulty),
            ExamSetId = dto.ExamSetId,
            IsPractice = dto.IsPractice
        };
    }

    private WritingQuestionDto MapToDto(WritingQuestion entity)
    {
        return new WritingQuestionDto
        {
            Id = entity.Id,
            TaskNumber = entity.TaskNumber,
            TaskType = entity.TaskType,
            PromptText = entity.PromptText,
            PromptImageUrl = entity.PromptImageUrl,
            GivenWords = entity.GivenWords,
            EmailContent = entity.EmailContent,
            EmailQuestions = entity.EmailQuestions,
            TimeLimit = entity.TimeLimit,
            MinWords = entity.MinWords,
            MaxWords = entity.MaxWords,
            MaxScore = entity.MaxScore,
            ScoringCriteria = entity.ScoringCriteria,
            SampleAnswer = entity.SampleAnswer,
            SampleAnswerTranslation = entity.SampleAnswerTranslation,
            ExplanationVietnamese = entity.ExplanationVietnamese,
            Topic = entity.Topic,
            Difficulty = entity.Difficulty,
            ExamSetId = entity.ExamSetId,
            IsPractice = entity.IsPractice
        };
    }
}
