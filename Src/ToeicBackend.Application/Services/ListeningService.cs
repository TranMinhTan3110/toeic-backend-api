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

    public async Task<IEnumerable<ListeningQuestionDto>> GetAllQuestionsAdminAsync()
    {
        var entities = (await _repository.GetAllQuestionsAdminAsync()).ToList();
        var dtos = entities.Select(MapToQuestionDto).ToList();
        var dtoMap = dtos.ToDictionary(q => q.Id);

        var p3Groups = await _repository.GetGroupsByPartAsync(3);
        var p4Groups = await _repository.GetGroupsByPartAsync(4);
        var allGroups = p3Groups.Concat(p4Groups).ToList();

        foreach (var group in allGroups)
        {
            if (group.QuestionIds == null) continue;

            foreach (var qId in group.QuestionIds)
            {
                if (dtoMap.TryGetValue(qId, out var dto))
                {
                    dto.Script = group.Script;
                    dto.GroupId = group.Id;
                }
                // Fallback cho trường hợp ID trong group thiếu prefix 'p_' so với document thật
                else if (dtoMap.TryGetValue("p_" + qId, out var pDto))
                {
                    pDto.Script = group.Script;
                    pDto.GroupId = group.Id;
                }
            }
        }

        return dtos;
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

    private ListeningGroupDto MapToGroupDto(QuestionGroup entity)
    {
        return new ListeningGroupDto
        {
            Id = entity.Id,
            Part = entity.Part,
            PassageText = entity.PassageText,
            Script = entity.Script,
            ImageUrl = entity.ImageUrl,
            AudioUrl = entity.AudioUrl,
            Source = entity.Source
        };
    }

    public Task<string> AddQuestionAsync(ListeningQuestion question)
    {
        return _repository.AddQuestionAsync(question);
    }

    public Task<string> AddGroupAsync(QuestionGroup group)
    {
        return _repository.AddGroupAsync(group);
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
