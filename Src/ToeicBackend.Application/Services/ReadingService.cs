using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class ReadingService : IReadingService
{
    private readonly IReadingRepository _repository;
    private readonly IMemoryCache _cache;

    // Cache data for 30 minutes to reduce Firebase reads significantly.
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan CountCacheDuration = TimeSpan.FromHours(1);

    private static string QuestionsCacheKey(int part) => $"reading_questions_part_{part}";
    private static string GroupsCacheKey(int part) => $"reading_groups_part_{part}";
    private static string CountCacheKey(int part) => $"reading_count_part_{part}";

    public ReadingService(IReadingRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<IEnumerable<ReadingQuestionDto>> GetQuestionsByPartAsync(int part)
    {
        var cacheKey = QuestionsCacheKey(part);
        if (_cache.TryGetValue(cacheKey, out List<ReadingQuestionDto>? cached) && cached != null)
            return cached;

        var entities = await _repository.GetQuestionsByPartAsync(part);
        var result = entities.Select(MapToQuestionDto).ToList();
        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<IEnumerable<ReadingQuestionDto>> GetAllQuestionsAdminAsync()
    {
        var entities = (await _repository.GetAllQuestionsAdminAsync()).ToList();
        var dtos = entities.Select(MapToQuestionDto).ToList();
        var dtoMap = dtos.ToDictionary(q => q.Id);

        var p6Groups = await _repository.GetGroupsByPartAsync(6);
        var p7Groups = await _repository.GetGroupsByPartAsync(7);
        var allGroups = p6Groups.Concat(p7Groups).ToList();

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
                else if (dtoMap.TryGetValue("p_" + qId, out var pDto))
                {
                    pDto.Script = group.Script;
                    pDto.GroupId = group.Id;
                }
            }
        }

        return dtos;
    }

    public async Task<IEnumerable<ReadingGroupDto>> GetGroupsByPartAsync(int part)
    {
        var cacheKey = GroupsCacheKey(part);
        if (_cache.TryGetValue(cacheKey, out List<ReadingGroupDto>? cachedGroups) && cachedGroups != null)
            return cachedGroups;

        var groups = (await _repository.GetGroupsByPartAsync(part)).ToList();
        if (groups.Count == 0) return Enumerable.Empty<ReadingGroupDto>();

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

            return new ReadingGroupDto
            {
                Id = group.Id,
                Part = group.Part,
                PassageText = group.PassageText,
                Script = group.Script,
                ImageUrl = group.ImageUrl,
                Source = group.Source,
                Questions = orderedQuestions
            };
        }).ToList();

        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<string> AddQuestionAsync(ReadingQuestion question)
    {
        var result = await _repository.AddQuestionAsync(question);
        _cache.Remove(QuestionsCacheKey(question.Part));
        _cache.Remove(CountCacheKey(question.Part));
        return result;
    }

    public async Task<string> AddGroupAsync(QuestionGroup group)
    {
        var result = await _repository.AddGroupAsync(group);
        _cache.Remove(GroupsCacheKey(group.Part));
        _cache.Remove(CountCacheKey(group.Part));
        return result;
    }

    public async Task<int> GetCountByPartAsync(int part)
    {
        var cacheKey = CountCacheKey(part);
        if (_cache.TryGetValue(cacheKey, out int cachedCount))
            return cachedCount;

        int count;
        if (part == 5)
            count = await _repository.GetQuestionCountByPartAsync(part);
        else
            count = await _repository.GetGroupCountByPartAsync(part);

        _cache.Set(cacheKey, count, CountCacheDuration);
        return count;
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

    public async Task<bool> UpdateQuestionAsync(string id, ReadingQuestionDto dto)
    {
        var question = await _repository.GetQuestionByIdAsync(id);
        if (question == null) return false;

        if (dto.QuestionText != null) question.QuestionText = dto.QuestionText;
        if (dto.Difficulty != null) question.Difficulty = dto.Difficulty;
        if (dto.CorrectAnswer != null) question.CorrectAnswer = dto.CorrectAnswer;
        if (dto.Options != null && dto.Options.Any()) question.Options = dto.Options;
        if (dto.ImageUrl != null) question.ImageUrl = dto.ImageUrl;
        if (dto.Explanation != null) question.Explanation = dto.Explanation;
        if (dto.ExplanationVi != null) question.ExplanationVi = dto.ExplanationVi;
        if (dto.GrammarTopicId != null) question.GrammarTopicId = dto.GrammarTopicId;

        if (dto.Script != null)
        {
            if (!string.IsNullOrEmpty(question.GroupId))
            {
                var group = await _repository.GetGroupByIdAsync(question.GroupId);
                if (group != null)
                {
                    group.Script = dto.Script;
                    await _repository.UpdateGroupAsync(group);
                    _cache.Remove(GroupsCacheKey(group.Part));
                }
            }
            question.Script = dto.Script;
        }

        var result = await _repository.UpdateQuestionAsync(question);
        if (result)
        {
            _cache.Remove(QuestionsCacheKey(question.Part));
            _cache.Remove(CountCacheKey(question.Part));
        }
        return result;
    }

    private ReadingQuestionDto MapToQuestionDto(ReadingQuestion entity)
    {
        return new ReadingQuestionDto
        {
            Id = entity.Id,
            Part = entity.Part,
            QuestionText = entity.QuestionText,
            ImageUrl = entity.ImageUrl,
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
}
