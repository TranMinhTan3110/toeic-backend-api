using System.Linq;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class WritingHistoryService : IWritingHistoryService
{
    private readonly IWritingHistoryRepository _repository;

    public WritingHistoryService(IWritingHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> SaveHistoryAsync(string userId, SaveWritingHistoryRequestDto request)
    {
        var questionIds = request.QuestionIds != null && request.QuestionIds.Any()
            ? request.QuestionIds
            : string.IsNullOrWhiteSpace(request.QuestionId)
                ? new List<string>()
                : new List<string> { request.QuestionId };

        var answers = request.Answers ?? new Dictionary<string, string>();
        if (!answers.Any() && !string.IsNullOrWhiteSpace(request.QuestionId))
        {
            answers[request.QuestionId] = request.UserAnswer ?? string.Empty;
        }

        var history = new WritingHistory
        {
            UserId = userId,
            QuestionId = request.QuestionId,
            TaskNumber = request.TaskNumber,
            TaskType = request.TaskType,
            QuestionIds = questionIds,
            QuestionCount = request.QuestionCount ?? Math.Max(questionIds.Count, 1),
            Answers = answers,
            SessionType = string.IsNullOrWhiteSpace(request.SessionType) ? "practice" : request.SessionType,
            UserAnswer = request.UserAnswer ?? string.Empty,
            WordCount = request.WordCount,
            TimeUsed = request.TimeUsed,
            AiScore = request.AiScore,
            AiFeedback = MapToEntityFeedback(request.AiFeedback),
            AiModel = request.AiModel,
            Status = DetermineDefaultStatus(request),
            ScoredAt = request.ScoredAt,
            ResultId = request.ResultId,
            SubmittedAt = DateTime.UtcNow
        };

        return await _repository.AddAsync(history);
    }

    public async Task<IEnumerable<WritingHistoryDto>> GetUserHistoryAsync(string userId, string? sessionType = null)
    {
        var entities = await _repository.GetByUserIdAsync(userId, sessionType);
        return entities.Select(MapToDto);
    }

    public async Task<WritingHistoryDto?> GetHistoryByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    private static string DetermineDefaultStatus(SaveWritingHistoryRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            return request.Status;
        }

        return request.AiScore.HasValue ? "scored" : "pending";
    }

    private static WritingHistoryDto MapToDto(WritingHistory entity)
    {
        return new WritingHistoryDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            QuestionId = entity.QuestionId,
            TaskNumber = entity.TaskNumber,
            TaskType = entity.TaskType,
            QuestionIds = entity.QuestionIds,
            QuestionCount = entity.QuestionCount,
            Answers = entity.Answers,
            SessionType = entity.SessionType,
            UserAnswer = entity.UserAnswer,
            WordCount = entity.WordCount,
            TimeUsed = entity.TimeUsed,
            AiScore = entity.AiScore,
            AiFeedback = MapToDtoFeedback(entity.AiFeedback),
            AiModel = entity.AiModel,
            Status = entity.Status,
            ScoredAt = entity.ScoredAt,
            ResultId = entity.ResultId,
            SubmittedAt = entity.SubmittedAt
        };
    }

    private static WritingAiFeedback? MapToEntityFeedback(WritingHistoryAiFeedbackDto? dto)
    {
        if (dto == null)
        {
            return null;
        }

        return new WritingAiFeedback
        {
            GrammarScore = dto.GrammarScore,
            VocabularyScore = dto.VocabularyScore,
            CohesionScore = dto.CohesionScore,
            CorrectionsVi = dto.CorrectionsVi,
            SuggestedImprovement = dto.SuggestedImprovement
        };
    }

    private static WritingHistoryAiFeedbackDto? MapToDtoFeedback(WritingAiFeedback? feedback)
    {
        if (feedback == null)
        {
            return null;
        }

        return new WritingHistoryAiFeedbackDto
        {
            GrammarScore = feedback.GrammarScore,
            VocabularyScore = feedback.VocabularyScore,
            CohesionScore = feedback.CohesionScore,
            CorrectionsVi = feedback.CorrectionsVi,
            SuggestedImprovement = feedback.SuggestedImprovement
        };
    }
}
