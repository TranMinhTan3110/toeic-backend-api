using ToeicBackend.Application.DTOs.Reading;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities.Reading;
using Microsoft.Extensions.Caching.Memory;
using ToeicBackend.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace ToeicBackend.Application.Services;

public class ReadingService : IReadingService
{
    private readonly IReadingRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ReadingService> _logger;

    private static string UserHistoryCacheKey(string userId) => $"user_reading_history_list_{userId}";
    private static string HistoryDetailCacheKey(string id) => $"reading_history_detail_{id}";
    private static readonly TimeSpan HistoryCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DetailCacheDuration = TimeSpan.FromMinutes(10);

    public ReadingService(IReadingRepository repository, IMemoryCache cache, ILogger<ReadingService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
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
            var selected = NormalizeSelectedOption(rawSelected, map.TryGetValue(qid, out var _ ) ? map[qid] : null);
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
                    // avoid spoilers in list
                    Explanation = null,
                    ExplanationVi = null,
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
                    ExplanationVi = null,
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
        var questions = await _repository.GetQuestionsByIdsAsync(ids);
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
            var selected = NormalizeSelectedOption(rawSelected, map.TryGetValue(qid, out var _ ) ? map[qid] : null);
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
        var questions = await _repository.GetQuestionsByIdsAsync(ids);
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
            var selected = NormalizeSelectedOption(rawSelected, map.TryGetValue(qid, out var _ ) ? map[qid] : null);
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
        var questions = await _repository.GetQuestionsByIdsAsync(ids);
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
            var selected = NormalizeSelectedOption(rawSelected, map.TryGetValue(qid, out var _ ) ? map[qid] : null);
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
    public async Task<string> SaveHistoryAsync(string userId, ToeicBackend.Application.DTOs.SaveReadingHistoryRequestDto request)
    {
        var entity = new ToeicBackend.Domain.Entities.Reading.ReadingHistory
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
                var questions = await _repository.GetQuestionsByIdsAsync(ids);
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

    public async Task<string> SavePart6HistoryAsync(string userId, ToeicBackend.Application.DTOs.SaveReadingHistoryRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        request.Part = 6;
        return await SaveHistoryAsync(userId, request);
    }

    public async Task<string> SavePart7HistoryAsync(string userId, ToeicBackend.Application.DTOs.SaveReadingHistoryRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        request.Part = 7;
        return await SaveHistoryAsync(userId, request);
    }

    public async Task<IEnumerable<ToeicBackend.Application.DTOs.ReadingHistoryDto>> GetPart7HistoryAsync(string userId)
    {
        var all = await GetUserHistoryAsync(userId);
        return all.Where(h => h.Part == 7);
    }

    public async Task<IEnumerable<ToeicBackend.Application.DTOs.ReadingHistoryDto>> GetPart6HistoryAsync(string userId)
    {
        // currently history stored with Part field; filter by part == 6
        var all = await GetUserHistoryAsync(userId);
        return all.Where(h => h.Part == 6);
    }

    public async Task<IEnumerable<ToeicBackend.Application.DTOs.ReadingHistoryDto>> GetUserHistoryAsync(string userId)
    {
        var cacheKey = UserHistoryCacheKey(userId);
        if (_cache.TryGetValue(cacheKey, out List<ToeicBackend.Application.DTOs.ReadingHistoryDto>? cached) && cached != null)
            return cached;

        var entities = await _repository.GetReadingHistoryByUserIdAsync(userId);
        var dtos = entities.Select(MapToHistoryDto).ToList();
        _cache.Set(cacheKey, dtos, HistoryCacheDuration);
        return dtos;
    }

    public async Task<ToeicBackend.Application.DTOs.ReadingHistoryDto?> GetHistoryByIdAsync(string id)
    {
        var cacheKey = HistoryDetailCacheKey(id);
        if (_cache.TryGetValue(cacheKey, out ToeicBackend.Application.DTOs.ReadingHistoryDto? cached) && cached != null)
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
                var questions = await _repository.GetQuestionsByIdsAsync(ids);
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

    private ToeicBackend.Application.DTOs.ReadingHistoryDto MapToHistoryDto(ToeicBackend.Domain.Entities.Reading.ReadingHistory entity)
    {
        return new ToeicBackend.Application.DTOs.ReadingHistoryDto
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
        // If raw looks like JSON, try to parse and extract readable fields
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
                // ignore and fall back to raw
            }
        }

        return raw;
    }
}
