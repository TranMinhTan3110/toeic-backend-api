using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class GrammarService : IGrammarService
{
    private readonly IGrammarRepository _repository;

    public GrammarService(IGrammarRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<GrammarTopicDto>> GetTopicsAsync()
    {
        var entities = await _repository.GetTopicsAsync();
        return entities.Select(MapToTopicDto);
    }

    public async Task<GrammarLessonDto?> GetLessonByTopicIdAsync(string topicId)
    {
        var entity = await _repository.GetLessonByTopicIdAsync(topicId);
        return entity == null ? null : MapToLessonDto(entity);
    }

    public async Task<IEnumerable<ListeningQuestionDto>> GetExercisesByTopicIdAsync(string topicId)
    {
        var entities = await _repository.GetQuestionsByTopicIdAsync(topicId);
        
        // Trộn ngẫu nhiên (shuffle) và lấy tối đa 10 câu trắc nghiệm
        var randomQuestions = entities
            .OrderBy(_ => Guid.NewGuid())
            .Take(10)
            .Select(MapToQuestionDto);

        return randomQuestions;
    }

    private GrammarTopicDto MapToTopicDto(GrammarTopic entity)
    {
        return new GrammarTopicDto
        {
            Id = entity.Id,
            Title = entity.Title,
            TitleEn = entity.TitleEn,
            Category = entity.Category,
            Description = entity.Description,
            Icon = entity.Icon,
            LessonCount = entity.LessonCount,
            ExerciseCount = entity.ExerciseCount,
            RelatedParts = entity.RelatedParts,
            Difficulty = entity.Difficulty,
            Order = entity.Order
        };
    }

    private GrammarLessonDto MapToLessonDto(GrammarLesson entity)
    {
        return new GrammarLessonDto
        {
            Id = entity.Id,
            TopicId = entity.TopicId,
            Title = entity.Title,
            Content = entity.Content,
            Order = entity.Order
        };
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
            IsForPractice = entity.IsForPractice
        };
    }
}
