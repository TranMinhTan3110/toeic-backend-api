using Microsoft.Extensions.Caching.Memory;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class ListeningService : IListeningService
{
    private readonly IListeningRepository _repository;
    private readonly IMemoryCache _cache;

    // Cache data trong 30 phút để giảm Firebase reads đáng kể.
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    private static string QuestionsCacheKey(int part) => $"listening_questions_part_{part}";
    private static string GroupsCacheKey(int part) => $"listening_groups_part_{part}";
    private static string CountCacheKey(int part) => $"listening_count_part_{part}";
    private static readonly TimeSpan CountCacheDuration = TimeSpan.FromHours(1);

    public ListeningService(IListeningRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<IEnumerable<ListeningQuestionDto>> GetQuestionsByPartAsync(int part)
    {
        var cacheKey = QuestionsCacheKey(part);
        if (_cache.TryGetValue(cacheKey, out List<ListeningQuestionDto>? cached) && cached != null)
            return cached;

        var entities = await _repository.GetQuestionsByPartAsync(part);
        var result = entities.Select(MapToQuestionDto).ToList();
        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<IEnumerable<ListeningQuestionDto>> GetAllQuestionsAdminAsync()
    {
        var entities = (await _repository.GetAllQuestionsAdminAsync()).ToList();
        var dtos = entities.Select(MapToQuestionDto).ToList();
        var dtoMap = dtos.ToDictionary(q => q.Id);

        var p3Groups = await _repository.GetGroupsByPartAsync(3);
        var p4Groups = await _repository.GetGroupsByPartAsync(4);
        var allGroups = p3Groups.Concat(p4Groups).ToList();

        foreach (var group in allGroups)
        {
            if (group.QuestionIds == null) continue;

            foreach (var qId in group.QuestionIds)
            {
                if (dtoMap.TryGetValue(qId, out var dto))
                {
                    dto.Script = group.Script;
                    dto.GroupId = group.Id;
                }
                // Fallback cho trường hợp ID trong group thiếu prefix 'p_' so với document thật
                else if (dtoMap.TryGetValue("p_" + qId, out var pDto))
                {
                    pDto.Script = group.Script;
                    pDto.GroupId = group.Id;
                }
            }
        }

        return dtos;
    }

    public async Task<IEnumerable<ListeningGroupDto>> GetGroupsByPartAsync(int part)
    {
        var cacheKey = GroupsCacheKey(part);
        if (_cache.TryGetValue(cacheKey, out List<ListeningGroupDto>? cachedGroups) && cachedGroups != null)
            return cachedGroups;

        var groups = (await _repository.GetGroupsByPartAsync(part)).ToList();
        if (groups.Count == 0) return Enumerable.Empty<ListeningGroupDto>();

        // Một lần query thay vì N lần (mỗi nhóm 1 request) — giảm latency Part 3/4.
        var allIds = groups.SelectMany(g => g.QuestionIds).Distinct().ToList();
        var allQuestions = (await _repository.GetQuestionsByIdsAsync(allIds)).ToList();
        var questionMap = allQuestions.ToDictionary(q => q.Id);

        var result = groups.Select(group =>
        {
            var orderedQuestions = group.QuestionIds
                .Where(id => questionMap.ContainsKey(id))
                .Select(id => questionMap[id])
                .Select(MapToQuestionDto)
                .ToList();

            return new ListeningGroupDto
            {
                Id = group.Id,
                Part = group.Part,
                PassageText = group.PassageText,
                Script = group.Script,
                ImageUrl = group.ImageUrl,
                AudioUrl = group.AudioUrl,
                Source = group.Source,
                Questions = orderedQuestions
            };
        }).ToList();

        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    private ListeningGroupDto MapToGroupDto(QuestionGroup entity)
    {
        return new ListeningGroupDto
        {
            Id = entity.Id,
            Part = entity.Part,
            PassageText = entity.PassageText,
            Script = entity.Script,
            ImageUrl = entity.ImageUrl,
            AudioUrl = entity.AudioUrl,
            Source = entity.Source
        };
    }

    public async Task<string> AddQuestionAsync(ListeningQuestion question)
    {
        var result = await _repository.AddQuestionAsync(question);
        // Xóa cache của part tương ứng để lần sau fetch data mới nhất.
        _cache.Remove(QuestionsCacheKey(question.Part));
        _cache.Remove(CountCacheKey(question.Part));
        return result;
    }

    public async Task<string> AddGroupAsync(QuestionGroup group)
    {
        var result = await _repository.AddGroupAsync(group);
        // Xóa cache của part tương ứng.
        _cache.Remove(GroupsCacheKey(group.Part));
        _cache.Remove(CountCacheKey(group.Part));
        return result;
    }

    /// <summary>
    /// Trả về số lượng câu/nhóm cực nhanh — dùng Count Aggregation (1 read).
    /// Cache 1 giờ vì count ít thay đổi hơn nội dung.
    /// </summary>
    public async Task<int> GetCountByPartAsync(int part)
    {
        var cacheKey = CountCacheKey(part);
        if (_cache.TryGetValue(cacheKey, out int cachedCount))
            return cachedCount;

        int count;
        if (part <= 2)
            count = await _repository.GetQuestionCountByPartAsync(part);
        else
            count = await _repository.GetGroupCountByPartAsync(part);

        _cache.Set(cacheKey, count, CountCacheDuration);
        return count;
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
            Skill = entity.Skill
        };
    }

    // --- History Practice Implementations ---
    private static string UserHistoryCacheKey(string userId) => $"user_listening_history_list_{userId}";
    private static string HistoryDetailCacheKey(string id) => $"listening_history_detail_{id}";
    private static readonly TimeSpan HistoryCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DetailCacheDuration = TimeSpan.FromMinutes(10);

    public async Task<string> SaveHistoryAsync(string userId, SaveListeningHistoryRequestDto request)
    {
        var entity = new ListeningHistory
        {
            UserId = userId,
            Part = request.Part,
            CorrectCount = request.CorrectCount,
            TotalCount = request.TotalCount,
            Percent = request.Percent,
            Date = DateTime.UtcNow,
            IncorrectQuestionIds = request.IncorrectQuestionIds ?? new(),
            SelectedAnswers = request.SelectedAnswers ?? new()
        };

        var id = await _repository.AddHistoryAsync(entity);
        entity.Id = id;

        // Clear user history list cache so next call fetches latest
        _cache.Remove(UserHistoryCacheKey(userId));

        // Pre-warm detail cache so detail click is instant
        _cache.Set(HistoryDetailCacheKey(id), MapToHistoryDto(entity), DetailCacheDuration);

        return id;
    }

    public async Task<IEnumerable<ListeningHistoryDto>> GetUserHistoryAsync(string userId)
    {
        var cacheKey = UserHistoryCacheKey(userId);
        if (_cache.TryGetValue(cacheKey, out List<ListeningHistoryDto>? cachedList) && cachedList != null)
        {
            return cachedList;
        }

        var entities = await _repository.GetHistoryByUserIdAsync(userId);
        var dtos = entities.Select(MapToHistoryDto).ToList();

        _cache.Set(cacheKey, dtos, HistoryCacheDuration);
        return dtos;
    }

    public async Task<ListeningHistoryDto?> GetHistoryByIdAsync(string id)
    {
        var cacheKey = HistoryDetailCacheKey(id);
        if (_cache.TryGetValue(cacheKey, out ListeningHistoryDto? cachedDetail) && cachedDetail != null)
        {
            return cachedDetail;
        }

        var entity = await _repository.GetHistoryByIdAsync(id);
        if (entity == null) return null;

        var dto = MapToHistoryDto(entity);
        _cache.Set(cacheKey, dto, DetailCacheDuration);
        return dto;
    }

    private ListeningHistoryDto MapToHistoryDto(ListeningHistory entity)
    {
        return new ListeningHistoryDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Part = entity.Part,
            CorrectCount = entity.CorrectCount,
            TotalCount = entity.TotalCount,
            Percent = entity.Percent,
            Date = entity.Date,
            IncorrectQuestionIds = entity.IncorrectQuestionIds ?? new(),
            SelectedAnswers = entity.SelectedAnswers ?? new()
        };
    }

    public async Task<bool> UpdateQuestionAsync(string id, ListeningQuestionDto dto)
    {
        var existing = await _repository.GetQuestionByIdAsync(id);
        if (existing == null) return false;

        var entity = new ListeningQuestion
        {
            Id           = id,
            Part         = dto.Part > 0 ? dto.Part : existing.Part,
            QuestionText = dto.QuestionText ?? existing.QuestionText,
            ImageUrl     = dto.ImageUrl ?? existing.ImageUrl,
            AudioUrl     = dto.AudioUrl ?? existing.AudioUrl,
            Options      = dto.Options ?? existing.Options,
            CorrectAnswer= dto.CorrectAnswer ?? existing.CorrectAnswer,
            Explanation  = dto.Explanation ?? existing.Explanation,
            ExplanationVi= dto.ExplanationVi ?? existing.ExplanationVi,
            Script       = dto.Script ?? existing.Script,
            GroupId      = dto.GroupId ?? existing.GroupId,
            Difficulty   = dto.Difficulty ?? existing.Difficulty,
            Skill        = dto.Skill ?? existing.Skill,
            IsForExam    = dto.IsForExam,
            IsForPractice= dto.IsForPractice,
        };

        var result = await _repository.UpdateQuestionAsync(id, entity);
        if (result)
        {
            _cache.Remove(QuestionsCacheKey(entity.Part));
            _cache.Remove(CountCacheKey(entity.Part));
        }
        return result;
    }

    public async Task<bool> DeleteQuestionAsync(string id)
    {
        var question = await _repository.GetQuestionByIdAsync(id);
        if (question == null) return false;

        var result = await _repository.DeleteQuestionAsync(id);
        if (result)
        {
            _cache.Remove(QuestionsCacheKey(question.Part));
            _cache.Remove(CountCacheKey(question.Part));
        }
        return result;
    }
}

