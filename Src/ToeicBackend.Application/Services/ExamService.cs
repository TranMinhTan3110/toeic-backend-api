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

    public async Task<IEnumerable<ExamDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<ExamDto?> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<ExamDto>> GetByFilterAsync(bool? isExam, bool? isPractice)
    {
        var entities = await _repository.GetByFilterAsync(isExam, isPractice);
        return entities.Select(MapToDto);
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

    private ExamDto MapToDto(Exam entity)
    {
        return new ExamDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Difficulty = entity.Difficulty,
            Duration = entity.Duration,
            Year = entity.Year,
            ImageUrl = entity.ImageUrl,
            AudioUrl = entity.AudioUrl,
            IsExam = entity.IsExam,
            IsPractice = entity.IsPractice,
            IsPremium = entity.IsPremium,
            IsPublished = entity.IsPublished,
            QuestionIds = entity.QuestionIds
        };
    }

    public async Task<string> SaveFullTestHistoryAsync(string userId, SaveFullTestRequestDto request)
    {
        var entity = new FullTestHistory
        {
            UserId = userId,
            ExamId = request.ExamId,
            ExamTitle = request.ExamTitle,
            ScoreListening = request.ScoreListening,
            ScoreReading = request.ScoreReading,
            TotalScore = request.TotalScore,
            CorrectCount = request.CorrectCount,
            TotalCount = request.TotalCount,
            TimeSpent = request.TimeSpent,
            CompletedAt = DateTime.UtcNow,
            Answers = request.Answers ?? new(),
            PartScores = request.PartScores ?? new()
        };

        var id = await _repository.AddFullTestHistoryAsync(entity);
        return id;
    }

    public async Task<IEnumerable<FullTestHistoryDto>> GetUserFullTestHistoryAsync(string userId)
    {
        var entities = await _repository.GetFullTestHistoryByUserIdAsync(userId);
        return entities.Select(MapToFullTestHistoryDto);
    }

    public async Task<FullTestHistoryDto?> GetFullTestHistoryByIdAsync(string id)
    {
        var entity = await _repository.GetFullTestHistoryByIdAsync(id);
        return entity == null ? null : MapToFullTestHistoryDto(entity);
    }

    private FullTestHistoryDto MapToFullTestHistoryDto(FullTestHistory entity)
    {
        return new FullTestHistoryDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            ExamId = entity.ExamId,
            ExamTitle = entity.ExamTitle,
            ScoreListening = entity.ScoreListening,
            ScoreReading = entity.ScoreReading,
            TotalScore = entity.TotalScore,
            CorrectCount = entity.CorrectCount,
            TotalCount = entity.TotalCount,
            TimeSpent = entity.TimeSpent,
            CompletedAt = entity.CompletedAt,
            Answers = entity.Answers,
            PartScores = entity.PartScores
        };
    }
}
