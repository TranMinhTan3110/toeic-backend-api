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

    public async Task<IEnumerable<SpeakingQuestionDto>> GetByTaskNumberAsync(int taskNumber)
    {
        var entities = await _repository.GetByTaskNumberAsync(taskNumber);
        return entities.Select(MapToDto);
    }

    public async Task<SpeakingQuestionDto?> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    private SpeakingQuestionDto MapToDto(SpeakingQuestion entity)
    {
        return new SpeakingQuestionDto
        {
            Id = entity.Id,
            TaskType = entity.TaskType,
            TaskNumber = entity.TaskNumber,
            PromptText = entity.PromptText,
            PromptImageUrl = entity.PromptImageUrl,
            PromptAudioUrl = entity.PromptAudioUrl,
            PreparationTime = entity.PreparationTime,
            ResponseTime = entity.ResponseTime,
            Difficulty = entity.Difficulty,
            AiPrompt = entity.AiPrompt,
            ScoringCriteria = entity.ScoringCriteria,
            ExamSetId = entity.ExamSetId,
            Topic = entity.Topic,
            IsPractice = entity.IsPractice,
            MaxScore = entity.MaxScore,
            SampleAnswer = entity.SampleAnswer,
            Questions = entity.Questions,
            AnswerTimes = entity.AnswerTimes,
            CreatedAt = entity.CreatedAt
        };
    }
}
