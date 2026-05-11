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
        var result = new List<ListeningGroupDto>();

        foreach (var group in groups)
        {
            var questions = await _repository.GetQuestionsByIdsAsync(group.QuestionIds);
            result.Add(new ListeningGroupDto
            {
                Id = group.Id,
                Part = group.Part,
                PassageText = group.PassageText,
                ImageUrl = group.ImageUrl,
                AudioUrl = group.AudioUrl,
                Source = group.Source,
                Questions = questions.OrderBy(q => q.Id).Select(MapToQuestionDto).ToList()
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
            GroupId = entity.GroupId,
            Difficulty = entity.Difficulty
        };
    }
}
