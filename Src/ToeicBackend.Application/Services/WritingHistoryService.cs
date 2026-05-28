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
        if (string.IsNullOrWhiteSpace(request.QuestionId))
        {
            throw new ArgumentException("QuestionId không được để trống");
        }

        var history = new WritingHistory
        {
            UserId = userId,
            QuestionId = request.QuestionId,
            TaskNumber = request.TaskNumber,
            TaskType = request.TaskType,
            SessionType = string.IsNullOrWhiteSpace(request.SessionType) ? "practice" : request.SessionType,
            UserAnswer = request.UserAnswer ?? string.Empty,
            WordCount = request.WordCount,
            TimeUsed = request.TimeUsed,
            AiScore = request.AiScore,
            AiFeedback = MapToEntityFeedback(request.AiFeedback),
            SubmittedAt = DateTime.UtcNow
        };

        return await _repository.AddAsync(history);
    }

    public async Task<string> SaveSessionAsync(string userId, SaveWritingSessionRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.Id))
        {
            var existing = await _repository.GetByIdAsync(request.Id);
            if (existing != null && existing.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật lịch sử writing này");
            }
        }

        var questionIds = (request.QuestionIds ?? new List<string>())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (questionIds.Count == 0)
        {
            throw new ArgumentException("QuestionIds không được để trống");
        }

        var answers = request.Answers?
            .Where(answer => !string.IsNullOrWhiteSpace(answer.Key))
            .ToDictionary(answer => answer.Key, answer => answer.Value ?? string.Empty);

        var firstQuestionId = questionIds[0];
        var firstAnswer = answers != null && answers.TryGetValue(firstQuestionId, out var answer)
            ? answer
            : string.Empty;

        var history = new WritingHistory
        {
            Id = request.Id ?? string.Empty,
            UserId = userId,
            QuestionId = firstQuestionId,
            TaskNumber = request.TaskNumber,
            TaskType = request.TaskType,
            SessionType = string.IsNullOrWhiteSpace(request.SessionType) ? "practice" : request.SessionType,
            UserAnswer = firstAnswer,
            QuestionIds = questionIds,
            Answers = answers,
            QuestionCount = request.QuestionCount,
            CorrectCount = request.CorrectCount,
            TimeSpent = request.TimeSpent,
            IncorrectIds = request.IncorrectIds?
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList(),
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

    private static WritingHistoryDto MapToDto(WritingHistory entity)
    {
        return new WritingHistoryDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            QuestionId = entity.QuestionId,
            TaskNumber = entity.TaskNumber,
            TaskType = entity.TaskType,
            SessionType = entity.SessionType,
            UserAnswer = entity.UserAnswer,
            WordCount = entity.WordCount,
            TimeUsed = entity.TimeUsed,
            AiScore = entity.AiScore,
            AiFeedback = MapToDtoFeedback(entity.AiFeedback),
            SubmittedAt = entity.SubmittedAt,
            QuestionIds = entity.QuestionIds,
            Answers = entity.Answers,
            QuestionCount = entity.QuestionCount,
            CorrectCount = entity.CorrectCount,
            TimeSpent = entity.TimeSpent,
            IncorrectIds = entity.IncorrectIds
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
