using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.DTOs.Reading;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;
using ToeicBackend.Domain.Entities.Reading;

namespace ToeicBackend.Application.Services;

public class ReadingService : IReadingService
{
    private readonly IReadingRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ReadingService> _logger;

    // Cache durations
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan CountCacheDuration = TimeSpan.FromHours(1);
    private static readonly TimeSpan HistoryCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DetailCacheDuration = TimeSpan.FromMinutes(10);

    // Cache keys
    private static string QuestionsCacheKey(int part) => $"reading_questions_part_{part}";
    private static string GroupsCacheKey(int part) => $"reading_groups_part_{part}";
    private static string CountCacheKey(int part) => $"reading_count_part_{part}";
    private static string UserHistoryCacheKey(string userId) => $"user_reading_history_list_{userId}";
    private static string HistoryDetailCacheKey(string id) => $"reading_history_detail_{id}";

    public ReadingService(IReadingRepository repository, IMemoryCache cache, ILogger<ReadingService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
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

    private static string NormalizeSelectedOption(string rawSelected, Part5Question? question)
    {
        if (string.IsNullOrWhiteSpace(rawSelected)) return string.Empty;
        var s = rawSelected.Trim();
        // If it's a single letter like 'A' or 'b'
        if (s.Length == 1 && "ABCDabcd".Contains(s[0])) return s.ToString().ToUpperInvariant();
        // If it starts with letter + dot or bracket like 'A. ' or 'A) '
        if (s.Length >= 2 && "ABCDabcd".Contains(s[0]) && (s[1] == '.' || s[1] == ')' || char.IsWhiteSpace(s[1])))
            return s[0].ToString().ToUpperInvariant();

        // If question provided, try to match by option text
        if (question != null)
        {
            string Compare(string? opt) => (opt ?? string.Empty).Trim().ToUpperInvariant();
            var cand = s.ToUpperInvariant();
            if (Compare(question.OptionA) == cand || ("A. " + Compare(question.OptionA)) == cand || ("A." + Compare(question.OptionA)) == cand) return "A";
            if (Compare(question.OptionB) == cand || ("B. " + Compare(question.OptionB)) == cand || ("B." + Compare(question.OptionB)) == cand) return "B";
            if (Compare(question.OptionC) == cand || ("C. " + Compare(question.OptionC)) == cand || ("C." + Compare(question.OptionC)) == cand) return "C";
            if (Compare(question.OptionD) == cand || ("D. " + Compare(question.OptionD)) == cand || ("D." + Compare(question.OptionD)) == cand) return "D";
        }

        // fallback: if starts with option text with prefix like 'A. over' or 'A.over'
        if (s.Length > 0 && "ABCDabcd".Contains(s[0])) return s[0].ToString().ToUpperInvariant();
        return s.ToUpperInvariant();
    }

    public async Task<IEnumerable<Part5QuestionDto>> GetPart5QuestionsAsync(int? count = null)
    {
        var take = (count.HasValue && count.Value > 0) ? count.Value : int.MaxValue;
        var entities = await _repository.GetRandomPart5QuestionsAsync(take);
        return entities.Select(e => new Part5QuestionDto
        {
            Id = e.Id,
            QuestionText = e.QuestionText,
            OptionA = e.OptionA,
            OptionB = e.OptionB,
            OptionC = e.OptionC,
            OptionD = e.OptionD,
            CorrectAnswer = e.CorrectAnswer,
            // include structured explanation fields to let frontend show translation/grammar after attempt
            Translation = e.Translation,
            GrammarExplanation = e.GrammarExplanation,
            GrammarPoint = e.GrammarPoint,
            OptionExplanations = e.OptionExplanations,
            // keep human-readable Explanation null to avoid spoilers in list view
            Explanation = null,
            ExplanationVi = e.ExplanationVi
        });
    }

    public async Task<Part5SubmitResponseDto> SubmitPart5AnswersAsync(Part5SubmitRequestDto request)
    {
        var ids = request.Answers.Keys;
        var questions = await _repository.GetPart5QuestionsByIdsAsync(ids);
        var map = questions.ToDictionary(q => q.Id, q => q);

        var response = new Part5SubmitResponseDto
        {
            TotalQuestions = request.Answers.Count,
            CorrectCount = 0
        };

        foreach (var kv in request.Answers)
        {
            var qid = kv.Key;
            var rawSelected = kv.Value?.Trim() ?? string.Empty;
            var selected = NormalizeSelectedOption(rawSelected, map.TryGetValue(qid, out var _) ? map[qid] : null);
            if (map.TryGetValue(qid, out var question))
            {
                try { _logger?.LogDebug("Grading q={QuestionId} rawSelected={Raw} normalized={Norm} correctAnswer={Correct}", qid, rawSelected, selected, question.CorrectAnswer); } catch {}
                var isCorrect = string.Equals(selected, question.CorrectAnswer?.Trim().ToUpperInvariant(), StringComparison.Ordinal);
                if (isCorrect) response.CorrectCount++;

                response.Results.Add(new Part5QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedOption = selected,
                    IsCorrect = isCorrect,
                    Explanation = NormalizeExplanation(question.Explanation),
                    ExplanationVi = question.ExplanationVi,
                    CorrectAnswer = question.CorrectAnswer
                });
            }
            else
            {
                response.Results.Add(new Part5QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedOption = selected,
                    IsCorrect = false,
                    Explanation = "Question not found",
                    ExplanationVi = null,
                    CorrectAnswer = null
                });
            }
        }

        return response;
    }

    public async Task<IEnumerable<Part6PassageDto>> GetPart6PassagesAsync()
    {
        var groups = await _repository.GetPart6PassagesAsync();
        var result = new List<Part6PassageDto>();
        foreach (var g in groups)
        {
            var dto = new Part6PassageDto
            {
                Id = g.Id,
                PassageText = g.PassageText,
                PassageTranslation = g.PassageTranslation,
                ImageUrl = g.ImageUrl,
                AudioUrl = g.AudioUrl,
                Passages = g.Passages?.Cast<object>().ToList()
            };

            foreach (var q in g.Questions)
            {
                dto.Questions.Add(new Part6QuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    // avoid full explanation text but include Vietnamese explanation when present
                    Explanation = null,
                    ExplanationVi = q.ExplanationVi,
                    CorrectAnswer = q.CorrectAnswer,
                    Translation = q.Translation,
                    GrammarExplanation = q.GrammarExplanation,
                    GrammarPoint = q.GrammarPoint,
                    OptionExplanations = q.OptionExplanations
                });
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task<IEnumerable<Part7PassageDto>> GetPart7PassagesAsync()
    {
        var groups = await _repository.GetPart7PassagesAsync();
        var result = new List<Part7PassageDto>();
        foreach (var g in groups)
        {
            var dto = new Part7PassageDto
            {
                Id = g.Id,
                PassageText = g.PassageText,
                PassageTranslation = g.PassageTranslation,
                ImageUrl = g.ImageUrl,
                AudioUrl = g.AudioUrl,
                Passages = g.Passages?.Cast<object>().ToList()
            };

            foreach (var q in g.Questions)
            {
                dto.Questions.Add(new Part7QuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    Explanation = null,
                    ExplanationVi = q.ExplanationVi,
                    CorrectAnswer = q.CorrectAnswer,
                    Translation = q.Translation,
                    GrammarExplanation = q.GrammarExplanation,
                    GrammarPoint = q.GrammarPoint,
                    OptionExplanations = q.OptionExplanations
                });
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task<Part6SubmitResponseDto> SubmitPart6AnswersAsync(Part6SubmitRequestDto request)
    {
        var ids = request.Answers.Keys;
        var questions = await _repository.GetQuestionsAsPart5ByIdsAsync(ids);
        var map = questions.ToDictionary(q => q.Id, q => q);

        var response = new Part6SubmitResponseDto
        {
            TotalQuestions = request.Answers.Count,
            CorrectCount = 0
        };

        foreach (var kv in request.Answers)
        {
            var qid = kv.Key;
            var rawSelected = kv.Value?.Trim() ?? string.Empty;
            var selected = NormalizeSelectedOption(rawSelected, map.TryGetValue(qid, out var _) ? map[qid] : null);
            if (map.TryGetValue(qid, out var question))
            {
                try { _logger?.LogDebug("Grading q={QuestionId} rawSelected={Raw} normalized={Norm} correctAnswer={Correct}", qid, rawSelected, selected, question.CorrectAnswer); } catch {}
                var isCorrect = string.Equals(selected, question.CorrectAnswer?.Trim().ToUpperInvariant(), StringComparison.Ordinal);
                if (isCorrect) response.CorrectCount++;

                response.Results.Add(new Part6QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedAnswer = selected,
                    IsCorrect = isCorrect,
                    Explanation = NormalizeExplanation(question.Explanation),
                    ExplanationVi = question.ExplanationVi,
                    CorrectAnswer = question.CorrectAnswer,
                    Translation = question.Translation,
                    GrammarExplanation = question.GrammarExplanation,
                    GrammarPoint = question.GrammarPoint,
                    OptionExplanations = question.OptionExplanations
                });
            }
            else
            {
                response.Results.Add(new Part6QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedAnswer = selected,
                    IsCorrect = false,
                    Explanation = "Question not found",
                    ExplanationVi = null,
                    CorrectAnswer = null
                });
            }
        }

        return response;
    }

    public async Task<IEnumerable<Part6PassageDto>> GetPart6QuestionsAsync()
    {
        return await GetPart6PassagesAsync();
    }

    public async Task<IEnumerable<Part7PassageDto>> GetPart7QuestionsAsync()
    {
        return await GetPart7PassagesAsync();
    }

    public async Task<Part5SubmitResponseDto> SubmitPart7AnswersAsync(Part5SubmitRequestDto request)
    {
        // Reuse Part7 grading same as Part6: fetch without part filter
        var ids = request.Answers.Keys;
        var questions = await _repository.GetQuestionsAsPart5ByIdsAsync(ids);
        var map = questions.ToDictionary(q => q.Id, q => q);

        var response = new Part5SubmitResponseDto
        {
            TotalQuestions = request.Answers.Count,
            CorrectCount = 0
        };

        foreach (var kv in request.Answers)
        {
            var qid = kv.Key;
            var rawSelected = kv.Value?.Trim() ?? string.Empty;
            var selected = NormalizeSelectedOption(rawSelected, map.TryGetValue(qid, out var _) ? map[qid] : null);
            if (map.TryGetValue(qid, out var question))
            {
                var isCorrect = string.Equals(selected, question.CorrectAnswer?.Trim().ToUpperInvariant(), StringComparison.Ordinal);
                if (isCorrect) response.CorrectCount++;

                response.Results.Add(new Part5QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedOption = selected,
                    IsCorrect = isCorrect,
                    Explanation = NormalizeExplanation(question.Explanation),
                    ExplanationVi = question.ExplanationVi,
                    CorrectAnswer = question.CorrectAnswer
                });
            }
            else
            {
                response.Results.Add(new Part5QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedOption = selected,
                    IsCorrect = false,
                    Explanation = "Question not found",
                    ExplanationVi = null,
                    CorrectAnswer = null
                });
            }
        }

        return response;
    }

    public async Task<Part7SubmitResponseDto> SubmitPart7AnswersAsync(Part7SubmitRequestDto request)
    {
        var ids = request.Answers.Keys;
        var questions = await _repository.GetQuestionsAsPart5ByIdsAsync(ids);
        var map = questions.ToDictionary(q => q.Id, q => q);

        var response = new Part7SubmitResponseDto
        {
            TotalQuestions = request.Answers.Count,
            CorrectCount = 0
        };

        foreach (var kv in request.Answers)
        {
            var qid = kv.Key;
            var rawSelected = kv.Value?.Trim() ?? string.Empty;
            var selected = NormalizeSelectedOption(rawSelected, map.TryGetValue(qid, out var _) ? map[qid] : null);
            if (map.TryGetValue(qid, out var question))
            {
                try { _logger?.LogDebug("Grading q={QuestionId} rawSelected={Raw} normalized={Norm} correctAnswer={Correct}", qid, rawSelected, selected, question.CorrectAnswer); } catch {}
                var isCorrect = string.Equals(selected, question.CorrectAnswer?.Trim().ToUpperInvariant(), StringComparison.Ordinal);
                if (isCorrect) response.CorrectCount++;

                response.Results.Add(new Part7QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedAnswer = selected,
                    IsCorrect = isCorrect,
                    Explanation = NormalizeExplanation(question.Explanation),
                    ExplanationVi = question.ExplanationVi,
                    CorrectAnswer = question.CorrectAnswer,
                    Translation = question.Translation,
                    GrammarExplanation = question.GrammarExplanation,
                    GrammarPoint = question.GrammarPoint,
                    OptionExplanations = question.OptionExplanations
                });
            }
            else
            {
                response.Results.Add(new Part7QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedAnswer = selected,
                    IsCorrect = false,
                    Explanation = "Question not found",
                    ExplanationVi = null,
                    CorrectAnswer = null
                });
            }
        }

        return response;
    }

    // --- History Practice for Reading ---
    public async Task<string> SaveHistoryAsync(string userId, SaveReadingHistoryRequestDto request)
    {
        var entity = new ReadingHistory
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

        var id = await _repository.AddReadingHistoryAsync(entity);
        entity.Id = id;

        _cache.Remove(UserHistoryCacheKey(userId));
        var dto = MapToHistoryDto(entity);
        // populate question details for this history so UI can show explanations
        try
        {
            var ids = dto.IncorrectQuestionIds.Concat(dto.SelectedAnswers.Keys).Distinct().ToList();
            if (ids.Count > 0)
            {
                var questions = await _repository.GetQuestionsAsPart5ByIdsAsync(ids);
                foreach (var q in questions)
                {
                    dto.QuestionDetails[q.Id] = new Part5QuestionDto
                    {
                        Id = q.Id,
                        QuestionText = q.QuestionText,
                        OptionA = q.OptionA,
                        OptionB = q.OptionB,
                        OptionC = q.OptionC,
                        OptionD = q.OptionD,
                        Explanation = NormalizeExplanation(q.Explanation),
                        ExplanationVi = q.ExplanationVi
                    };
                }
            }
        }
        catch
        {
            // ignore question detail population errors
        }

        _cache.Set(HistoryDetailCacheKey(id), dto, DetailCacheDuration);
        return id;
    }

    public async Task<string> SavePart6HistoryAsync(string userId, SaveReadingHistoryRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        request.Part = 6;
        return await SaveHistoryAsync(userId, request);
    }

    public async Task<string> SavePart7HistoryAsync(string userId, SaveReadingHistoryRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        request.Part = 7;
        return await SaveHistoryAsync(userId, request);
    }

    public async Task<IEnumerable<ReadingHistoryDto>> GetPart5HistoryAsync(string userId)
    {
        var all = await GetUserHistoryAsync(userId);
        return all.Where(h => h.Part == 5);
    }

    public async Task<IEnumerable<ReadingHistoryDto>> GetPart6HistoryAsync(string userId)
    {
        var all = await GetUserHistoryAsync(userId);
        return all.Where(h => h.Part == 6);
    }

    public async Task<IEnumerable<ReadingHistoryDto>> GetPart7HistoryAsync(string userId)
    {
        var all = await GetUserHistoryAsync(userId);
        return all.Where(h => h.Part == 7);
    }

    public async Task<IEnumerable<ReadingHistoryDto>> GetUserHistoryAsync(string userId)
    {
        var cacheKey = UserHistoryCacheKey(userId);
        if (_cache.TryGetValue(cacheKey, out List<ReadingHistoryDto>? cached) && cached != null)
            return cached;

        var entities = await _repository.GetReadingHistoryByUserIdAsync(userId);
        var dtos = entities.Select(MapToHistoryDto).ToList();
        _cache.Set(cacheKey, dtos, HistoryCacheDuration);
        return dtos;
    }

    public async Task<ReadingHistoryDto?> GetHistoryByIdAsync(string id)
    {
        var cacheKey = HistoryDetailCacheKey(id);
        if (_cache.TryGetValue(cacheKey, out ReadingHistoryDto? cached) && cached != null)
            return cached;

        var entity = await _repository.GetReadingHistoryByIdAsync(id);
        if (entity == null) return null;
        var dto = MapToHistoryDto(entity);
        // populate question details for UI
        try
        {
            var ids = dto.IncorrectQuestionIds.Concat(dto.SelectedAnswers.Keys).Distinct().ToList();
            if (ids.Count > 0)
            {
                var questions = await _repository.GetQuestionsAsPart5ByIdsAsync(ids);
                foreach (var q in questions)
                {
                    dto.QuestionDetails[q.Id] = new Part5QuestionDto
                    {
                        Id = q.Id,
                        QuestionText = q.QuestionText,
                        OptionA = q.OptionA,
                        OptionB = q.OptionB,
                        OptionC = q.OptionC,
                        OptionD = q.OptionD,
                        Explanation = NormalizeExplanation(q.Explanation),
                        ExplanationVi = q.ExplanationVi
                    };
                }
            }
        }
        catch
        {
            // ignore
        }

        _cache.Set(cacheKey, dto, DetailCacheDuration);
        return dto;
    }

    private ReadingHistoryDto MapToHistoryDto(ReadingHistory entity)
    {
        return new ReadingHistoryDto
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

    private static string? NormalizeExplanation(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        raw = raw.Trim();
        if (raw.StartsWith("{") || raw.StartsWith("["))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(raw);
                var root = doc.RootElement;
                var parts = new List<string>();
                if (root.TryGetProperty("grammar_explanation", out var ge) && ge.ValueKind == System.Text.Json.JsonValueKind.String)
                    parts.Add(System.Net.WebUtility.HtmlDecode(ge.GetString() ?? ""));
                if (root.TryGetProperty("grammar_point", out var gp) && gp.ValueKind == System.Text.Json.JsonValueKind.String)
                    parts.Insert(0, gp.GetString() ?? "");
                if (root.TryGetProperty("translation", out var tr) && tr.ValueKind == System.Text.Json.JsonValueKind.String)
                    parts.Add("Translation: " + (tr.GetString() ?? ""));
                if (root.TryGetProperty("option_explanations", out var oe) && oe.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    var opts = new List<string>();
                    foreach (var p in oe.EnumerateObject())
                    {
                        if (p.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                            opts.Add($"{p.Name}: {p.Value.GetString()}");
                    }
                    if (opts.Count > 0) parts.Add("Options: " + string.Join("; ", opts));
                }

                if (parts.Count > 0) return string.Join("\n", parts.Where(s => !string.IsNullOrWhiteSpace(s)));
            }
            catch
            {
                // ignore
            }
        }

        return raw;
    }
}
