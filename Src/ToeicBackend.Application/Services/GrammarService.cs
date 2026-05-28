using Microsoft.Extensions.Caching.Memory;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class GrammarService : IGrammarService
{
    private readonly IGrammarRepository _repository;
    private readonly IMemoryCache _cache;

    private const string TopicsCacheKey = "grammar_topics_list";
    private static string LessonCacheKey(string topicId) => $"grammar_lesson_{topicId}";
    private static string ExercisesCacheKey(string topicId) => $"grammar_exercises_{topicId}";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public GrammarService(IGrammarRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<IEnumerable<GrammarTopicDto>> GetTopicsAsync()
    {
        if (_cache.TryGetValue(TopicsCacheKey, out List<GrammarTopicDto>? cached) && cached != null)
        {
            Console.WriteLine("[CACHE] Returning grammar topics list from MemoryCache");
            return cached;
        }

        var entities = await _repository.GetTopicsAsync();
        var result = entities.Select(MapToTopicDto).ToList();
        _cache.Set(TopicsCacheKey, result, CacheDuration);
        return result;
    }

    public async Task<GrammarLessonDto?> GetLessonByTopicIdAsync(string topicId)
    {
        var cacheKey = LessonCacheKey(topicId);
        if (_cache.TryGetValue(cacheKey, out GrammarLessonDto? cached) && cached != null)
        {
            Console.WriteLine($"[CACHE] Returning grammar lesson for topic '{topicId}' from MemoryCache");
            return cached;
        }

        var entity = await _repository.GetLessonByTopicIdAsync(topicId);
        if (entity == null) return null;

        var result = MapToLessonDto(entity);
        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<IEnumerable<ListeningQuestionDto>> GetExercisesByTopicIdAsync(string topicId, bool random = false, int limit = 0)
    {
        var cacheKey = ExercisesCacheKey(topicId);
        if (!_cache.TryGetValue(cacheKey, out List<ListeningQuestion>? cachedExercises) || cachedExercises == null)
        {
            Console.WriteLine($"[CACHE] Exercises cache miss for topic '{topicId}'. Querying Firestore...");
            var entities = await _repository.GetQuestionsByTopicIdAsync(topicId);
            cachedExercises = entities.ToList();
            _cache.Set(cacheKey, cachedExercises, CacheDuration);
        }
        else
        {
            Console.WriteLine($"[CACHE] Returning grammar exercises list for topic '{topicId}' from MemoryCache");
        }

        // Sắp xếp mặc định theo ngày tạo (cũ nhất lên trước) để ổn định danh sách
        var query = cachedExercises.OrderBy(q => q.CreatedAt ?? DateTime.MinValue);
        
        if (random)
        {
            query = cachedExercises.OrderBy(_ => Guid.NewGuid());
        }

        IEnumerable<ListeningQuestion> result = query;
        if (limit > 0)
        {
            result = result.Take(limit);
        }

        return result.Select(MapToQuestionDto).ToList();
    }

    private GrammarTopicDto MapToTopicDto(GrammarTopic entity)
    {
        return new GrammarTopicDto
        {
            Id = entity.Id,
            Title = entity.Title,
            TitleEn = entity.TitleEn,
            Category = entity.Category,
            Description = entity.Description,
            Icon = entity.Icon,
            LessonCount = entity.LessonCount,
            ExerciseCount = entity.ExerciseCount,
            RelatedParts = entity.RelatedParts,
            Difficulty = entity.Difficulty,
            Order = entity.Order
        };
    }

    private GrammarLessonDto MapToLessonDto(GrammarLesson entity)
    {
        return new GrammarLessonDto
        {
            Id = entity.Id,
            TopicId = entity.TopicId,
            Title = entity.Title,
            Content = entity.Content,
            Order = entity.Order
        };
    }

    private ListeningQuestionDto MapToQuestionDto(ListeningQuestion entity)
    {
        return new ListeningQuestionDto
        {
            Id = entity.Id,
            Part = entity.Part,
            QuestionText = entity.QuestionText,
            ImageUrl = entity.ImageUrl,
            AudioUrl = entity.AudioUrl,
            Options = entity.Options,
            CorrectAnswer = entity.CorrectAnswer,
            Explanation = entity.Explanation,
            ExplanationVi = entity.ExplanationVi,
            Script = entity.Script,
            GroupId = entity.GroupId,
            Difficulty = entity.Difficulty,
            IsForExam = entity.IsForExam,
            IsForPractice = entity.IsForPractice,
            GrammarTopicId = entity.GrammarTopicId
        };
    }

    private void InvalidateTopicsCache()
    {
        Console.WriteLine($"[CACHE] Invalidating cache: '{TopicsCacheKey}'");
        _cache.Remove(TopicsCacheKey);
    }

    private void InvalidateLessonCache(string topicId)
    {
        Console.WriteLine($"[CACHE] Invalidating cache for topic '{topicId}' lesson");
        _cache.Remove(LessonCacheKey(topicId));
        InvalidateTopicsCache(); // LessonCount or Order might have changed
    }

    private void InvalidateExercisesCache(string topicId)
    {
        Console.WriteLine($"[CACHE] Invalidating cache for topic '{topicId}' exercises");
        _cache.Remove(ExercisesCacheKey(topicId));
        InvalidateTopicsCache(); // ExerciseCount changes
    }

    public async Task<GrammarTopicDto> CreateTopicAsync(CreateGrammarTopicDto dto)
    {
        var entity = new GrammarTopic
        {
            Title = dto.Title,
            TitleEn = dto.TitleEn,
            Category = dto.Category,
            Description = dto.Description,
            Icon = dto.Icon,
            Difficulty = dto.Difficulty,
            Order = dto.Order,
            IsPublished = dto.IsPublished,
            RelatedParts = dto.RelatedParts,
            LessonCount = 0,
            ExerciseCount = 0
        };

        var created = await _repository.CreateTopicAsync(entity);
        InvalidateTopicsCache();
        return MapToTopicDto(created);
    }

    public async Task<GrammarTopicDto?> UpdateTopicAsync(string id, CreateGrammarTopicDto dto)
    {
        var existing = await _repository.GetTopicByIdAsync(id);
        if (existing == null) return null;

        existing.Title = dto.Title;
        existing.TitleEn = dto.TitleEn;
        existing.Category = dto.Category;
        existing.Description = dto.Description;
        existing.Icon = dto.Icon;
        existing.Difficulty = dto.Difficulty;
        existing.Order = dto.Order;
        existing.IsPublished = dto.IsPublished;
        existing.RelatedParts = dto.RelatedParts;

        var updated = await _repository.UpdateTopicAsync(id, existing);
        InvalidateTopicsCache();
        return updated == null ? null : MapToTopicDto(updated);
    }

    public async Task<bool> DeleteTopicAsync(string id)
    {
        var result = await _repository.DeleteTopicAsync(id);
        if (result)
        {
            InvalidateTopicsCache();
            _cache.Remove(LessonCacheKey(id));
            _cache.Remove(ExercisesCacheKey(id));
        }
        return result;
    }

    public async Task<GrammarLessonDto> SaveLessonAsync(string topicId, CreateGrammarLessonDto dto)
    {
        var lesson = new GrammarLesson
        {
            TopicId = topicId,
            Title = dto.Title,
            Content = dto.Content,
            Order = dto.Order
        };

        var saved = await _repository.SaveLessonAsync(lesson);

        // Cập nhật số lượng bài học của topic lên 1
        var topic = await _repository.GetTopicByIdAsync(topicId);
        if (topic != null && topic.LessonCount != 1)
        {
            topic.LessonCount = 1;
            await _repository.UpdateTopicAsync(topicId, topic);
        }

        InvalidateLessonCache(topicId);
        return MapToLessonDto(saved);
    }

    public async Task<ListeningQuestionDto> AddExerciseAsync(string topicId, ListeningQuestionDto dto)
    {
        var question = new ListeningQuestion
        {
            Id = dto.Id,
            Part = dto.Part,
            QuestionText = dto.QuestionText,
            ImageUrl = dto.ImageUrl,
            AudioUrl = dto.AudioUrl,
            Options = dto.Options,
            CorrectAnswer = dto.CorrectAnswer,
            Explanation = dto.Explanation,
            ExplanationVi = dto.ExplanationVi,
            Script = dto.Script,
            GroupId = dto.GroupId,
            Difficulty = dto.Difficulty ?? "medium",
            IsForExam = dto.IsForExam,
            IsForPractice = true,
            Skill = "listening", // sharing question schema
            GrammarTopicId = topicId
        };

        var added = await _repository.AddExerciseAsync(question);

        // Tăng ExerciseCount của Topic
        var topic = await _repository.GetTopicByIdAsync(topicId);
        if (topic != null)
        {
            topic.ExerciseCount += 1;
            await _repository.UpdateTopicAsync(topicId, topic);
        }

        InvalidateExercisesCache(topicId);
        return MapToQuestionDto(added);
    }

    public async Task<ListeningQuestionDto?> UpdateExerciseAsync(string id, ListeningQuestionDto dto)
    {
        var existing = await _repository.GetQuestionsByTopicIdAsync(dto.GrammarTopicId ?? "");
        var target = existing.FirstOrDefault(q => q.Id == id);
        if (target == null) return null;

        target.Part = dto.Part;
        target.QuestionText = dto.QuestionText;
        target.ImageUrl = dto.ImageUrl;
        target.AudioUrl = dto.AudioUrl;
        target.Options = dto.Options;
        target.CorrectAnswer = dto.CorrectAnswer;
        target.Explanation = dto.Explanation;
        target.ExplanationVi = dto.ExplanationVi;
        target.Script = dto.Script;
        target.GroupId = dto.GroupId;
        target.Difficulty = dto.Difficulty ?? "medium";
        target.IsForExam = dto.IsForExam;

        var updated = await _repository.UpdateExerciseAsync(id, target);
        if (updated != null)
        {
            InvalidateExercisesCache(dto.GrammarTopicId ?? "");
        }
        return updated == null ? null : MapToQuestionDto(updated);
    }

    public async Task<bool> DeleteExerciseAsync(string id)
    {
        // Để invalidate cache của topicId tương ứng, tìm trong cache xem exercises của topic nào chứa exercise.Id này
        string? targetTopicId = null;
        var topics = await _repository.GetTopicsAsync();
        foreach (var topic in topics)
        {
            var exercisesKey = ExercisesCacheKey(topic.Id);
            if (_cache.TryGetValue(exercisesKey, out List<ListeningQuestion>? cached) && cached != null)
            {
                if (cached.Any(q => q.Id == id))
                {
                    targetTopicId = topic.Id;
                    break;
                }
            }
        }

        var result = await _repository.DeleteExerciseAsync(id);
        if (result)
        {
            InvalidateTopicsCache();
            if (!string.IsNullOrEmpty(targetTopicId))
            {
                InvalidateExercisesCache(targetTopicId);
            }
            else
            {
                // Fallback: nếu không tìm thấy cache của topic cụ thể, xoá cache exercises của tất cả topics
                foreach (var topic in topics)
                {
                    _cache.Remove(ExercisesCacheKey(topic.Id));
                }
            }
        }
        return result;
    }
}
