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
        var groups = (await _repository.GetGroupsByPartAsync(part)).ToList();
        if (groups.Count == 0) return Enumerable.Empty<ListeningGroupDto>();

        // Một lần query thay vì N lần (mỗi nhóm 1 request) — giảm latency Part 3/4.
        var allIds = groups.SelectMany(g => g.QuestionIds).Distinct().ToList();
        var allQuestions = (await _repository.GetQuestionsByIdsAsync(allIds)).ToList();
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
