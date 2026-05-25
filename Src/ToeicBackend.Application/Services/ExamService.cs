using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class ExamService : IExamService
{
    private readonly IExamRepository _repository;
    private readonly IListeningRepository _listeningRepository;

    public ExamService(IExamRepository repository, IListeningRepository listeningRepository)
    {
        _repository = repository;
        _listeningRepository = listeningRepository;
    }

    public async Task<IEnumerable<ListeningQuestionDto>> GetExamQuestionsAsync(string examId)
    {
        var entities = await _repository.GetExamQuestionsAsync(examId);
        return entities.Select(MapToQuestionDto);
    }

    public async Task<IEnumerable<ListeningGroupDto>> GetExamGroupsAsync(string examId)
    {
        var groups = (await _repository.GetExamGroupsAsync(examId)).ToList();
        if (groups.Count == 0) return Enumerable.Empty<ListeningGroupDto>();

        var allIds = groups.SelectMany(g => g.QuestionIds).Distinct().ToList();
        var allQuestions = (await _listeningRepository.GetQuestionsByIdsAsync(allIds)).ToList();
        var questionMap = allQuestions.ToDictionary(q => q.Id);

        return groups.Select(group =>
        {
            var orderedQuestions = group.QuestionIds
                .Where(id => questionMap.ContainsKey(id))
                .Select(id => questionMap[id])
                .Select(MapToQuestionDto)
                .ToList();

            return new ListeningGroupDto
            {
                Id = group.Id,
                Part = group.Part,
                PassageText = group.PassageText,
                Script = group.Script,
                ImageUrl = group.ImageUrl,
                AudioUrl = group.AudioUrl,
                Source = group.Source,
                Questions = orderedQuestions
            };
        });
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
