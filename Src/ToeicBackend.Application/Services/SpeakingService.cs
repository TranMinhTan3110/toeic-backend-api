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
}
