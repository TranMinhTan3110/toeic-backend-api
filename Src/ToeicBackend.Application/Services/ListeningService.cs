using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class ListeningService : IListeningService
{
    private readonly IListeningRepository _repository;

    public ListeningService(IListeningRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ListeningQuestionDto>> GetQuestionsByPartAsync(int part)
    {
        var entities = await _repository.GetQuestionsByPartAsync(part);
        return entities.Select(MapToQuestionDto);
    }

    public async Task<IEnumerable<ListeningGroupDto>> GetGroupsByPartAsync(int part)
    {
        var groups = await _repository.GetGroupsByPartAsync(part);
        var questions = await _repository.GetQuestionsByPartAsync(part);
        var questionMap = questions.ToDictionary(q => q.Id);
        
        var result = new List<ListeningGroupDto>();

        foreach (var group in groups)
        {
            var groupQuestions = new List<ListeningQuestionDto>();
            foreach (var qId in group.QuestionIds)
            {
                if (questionMap.TryGetValue(qId, out var q))
                {
                    groupQuestions.Add(MapToQuestionDto(q));
                }
            }

            result.Add(new ListeningGroupDto
            {
                Id = group.Id,
                Part = group.Part,
                PassageText = group.PassageText,
                Script = group.Script,
                ImageUrl = group.ImageUrl,
                AudioUrl = group.AudioUrl,
                Source = group.Source,
                Questions = groupQuestions.OrderBy(q => q.Id).ToList()
            });
        }

        return result;
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
