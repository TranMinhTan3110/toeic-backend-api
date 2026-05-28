using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class WritingQuestionService : IWritingQuestionService
{
    private readonly IWritingQuestionRepository _repository;

    public WritingQuestionService(IWritingQuestionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<WritingQuestionDto?> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetByTaskTypeAsync(string taskType)
    {
        var entities = await _repository.GetByTaskTypeAsync(taskType);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetByTaskNumberAsync(int taskNumber)
    {
        var entities = await _repository.GetByTaskNumberAsync(taskNumber);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetByDifficultyAsync(string difficulty)
    {
        var entities = await _repository.GetByDifficultyAsync(difficulty);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetPracticeQuestionsAsync()
    {
        var entities = await _repository.GetPracticeQuestionsAsync();
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetPracticeByTaskTypeAsync(string taskType)
    {
        var entities = await _repository.GetPracticeByTaskTypeAsync(taskType);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetExamByTaskTypeAsync(string taskType)
    {
        var entities = await _repository.GetExamByTaskTypeAsync(taskType);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetByExamSetIdAsync(string examSetId)
    {
        var entities = await _repository.GetByExamSetIdAsync(examSetId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<string>> GetAvailableTaskTypesAsync()
    {
        return await _repository.GetAvailableTaskTypesAsync();
    }

    public async Task<IEnumerable<WritingQuestionDto>> GetQuestionsByExamSetIdAsync(string examSetId)
    {
        var entities = await _repository.GetQuestionsByExamSetIdAsync(examSetId);
        return entities.Select(MapToDto);
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
