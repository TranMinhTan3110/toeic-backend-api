using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class SpeakingService : ISpeakingService
{
    private readonly ISpeakingRepository _repository;

    public SpeakingService(ISpeakingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SpeakingQuestionDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SpeakingQuestionDto>> GetByTaskNumberAsync(
        int taskNumber,
        bool? isExam = null,
        bool? isPractice = null)
    {
        var entities = await _repository.GetByTaskNumberAsync(taskNumber, isExam, isPractice);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SpeakingQuestionDto>> GetByFilterAsync(bool? isExam, bool? isPractice)
    {
        var entities = await _repository.GetByFilterAsync(isExam, isPractice);
        return entities.Select(MapToDto);
    }

    public async Task<int> GetCountByFilterAsync(bool? isExam, bool? isPractice)
    {
        return await _repository.GetCountByFilterAsync(isExam, isPractice);
    }

    public async Task<IEnumerable<SpeakingQuestionDto>> GetByExamSetIdAsync(string examSetId)
    {
        var entities = await _repository.GetByExamSetIdAsync(examSetId);
        
        if (entities == null || !entities.Any())
        {
            var baseId = examSetId;
            if (examSetId.EndsWith("_speaking", StringComparison.OrdinalIgnoreCase))
            {
                baseId = examSetId.Substring(0, examSetId.Length - 9);
            }
            else if (examSetId.EndsWith("_writing", StringComparison.OrdinalIgnoreCase))
            {
                baseId = examSetId.Substring(0, examSetId.Length - 8);
            }

            if (baseId != examSetId)
            {
                Console.WriteLine($"[DEBUG] No speaking questions found for '{examSetId}'. Trying base ID: '{baseId}'");
                var baseEntities = await _repository.GetByExamSetIdAsync(baseId);
                if (baseEntities != null && baseEntities.Any())
                {
                    entities = baseEntities;
                }
            }

            if (entities == null || !entities.Any())
            {
                string? alternateId = null;
                if (baseId.StartsWith("test_", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(baseId.Substring(5), out int num))
                    {
                        alternateId = $"ETS-SPK-2024-{num:D2}";
                    }
                }
                else if (baseId.StartsWith("ETS-SPK-2024-", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(baseId.Substring(13), out int num))
                    {
                        alternateId = $"test_{num}";
                    }
                }

                if (!string.IsNullOrEmpty(alternateId))
                {
                    Console.WriteLine($"[DEBUG] No speaking questions found for '{examSetId}' or '{baseId}'. Retrying with alternate ID: '{alternateId}'");
                    var altEntities = await _repository.GetByExamSetIdAsync(alternateId);
                    if (altEntities != null && altEntities.Any())
                    {
                        entities = altEntities;
                    }
                }
            }
        }
        
        return entities.Select(MapToDto);
    }


    public async Task<SpeakingQuestionDto?> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    private SpeakingQuestionDto MapToDto(SpeakingQuestion entity)
    {
        var dto = new SpeakingQuestionDto
        {
            Id = entity.Id,
            TaskType = entity.TaskType,
            TaskNumber = entity.TaskNumber,
            PromptText = entity.PromptText,
            PromptImageUrl = entity.PromptImageUrl,
            PromptAudioUrl = entity.PromptAudioUrl,
            ImageUrl = entity.ImageUrl,
            AudioUrl = entity.AudioUrl,
            PreparationTime = entity.PreparationTime,
            ResponseTime = entity.ResponseTime,
            Difficulty = entity.Difficulty,
            AiPrompt = entity.AiPrompt,
            ScoringCriteria = entity.ScoringCriteria,
            ExamSetId = entity.ExamSetId,
            Topic = entity.Topic,
            IsPractice = entity.IsPractice,
            IsExam = entity.IsExam,
            MaxScore = entity.MaxScore,
            SampleAnswer = entity.SampleAnswer,
            Questions = entity.Questions,
            AnswerTimes = entity.AnswerTimes,
            CreatedAt = entity.CreatedAt
        };

        // Map explanation if available
        if (entity.Explanation != null)
        {
            dto.Explanation = MapExplanationToDto(entity.Explanation);
        }

        return dto;
    }

    private SpeakingExplanationDto MapExplanationToDto(SpeakingExplanation explanation)
    {
        return new SpeakingExplanationDto
        {
            Translation = explanation.Translation,
            ContextTranslation = explanation.ContextTranslation,
            Keywords = explanation.Keywords.Select(k => new ExplanationKeywordDto
            {
                Word = k.Word,
                Ipa = k.Ipa,
                Meaning = k.Meaning
            }).ToList(),
            QuestionsTranslation = explanation.QuestionsTranslation,
            SampleAnswers = explanation.SampleAnswers,
            SampleAnswersTranslation = explanation.SampleAnswersTranslation
        };
    }

    public async Task AddQuestionAsync(SpeakingQuestion entity)
    {
        await _repository.AddAsync(entity);
    }

    public async Task<bool> UpdateQuestionAsync(string id, SpeakingQuestionDto dto)
    {
        var entity = new SpeakingQuestion
        {
            Id = id,
            TaskType        = dto.TaskType,
            TaskNumber      = dto.TaskNumber,
            PromptText      = dto.PromptText,
            PromptImageUrl  = dto.PromptImageUrl,
            PromptAudioUrl  = dto.PromptAudioUrl,
            ImageUrl        = dto.ImageUrl,
            AudioUrl        = dto.AudioUrl,
            PreparationTime = dto.PreparationTime,
            ResponseTime    = dto.ResponseTime,
            Difficulty      = dto.Difficulty ?? "medium",
            AiPrompt        = dto.AiPrompt ?? string.Empty,
            ScoringCriteria = dto.ScoringCriteria ?? new List<string>(),
            ExamSetId       = dto.ExamSetId,
            Topic           = dto.Topic,
            IsPractice      = dto.IsPractice,
            IsExam          = dto.IsExam,
            MaxScore        = dto.MaxScore,
            SampleAnswer    = dto.SampleAnswer,
            Questions       = dto.Questions ?? new List<string>(),
            AnswerTimes     = dto.AnswerTimes ?? new List<int>(),
        };
        return await _repository.UpdateAsync(id, entity);
    }

    public async Task<bool> DeleteQuestionAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }
}
