using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class ExamService : IExamService
{
    private readonly IExamRepository _repository;
    private readonly IListeningRepository _listeningRepository;
    private readonly ISpeakingRepository _speakingRepository;
    private readonly IWritingQuestionRepository _writingQuestionRepository;

    public ExamService(
        IExamRepository repository, 
        IListeningRepository listeningRepository,
        ISpeakingRepository speakingRepository,
        IWritingQuestionRepository writingQuestionRepository)
    {
        _repository = repository;
        _listeningRepository = listeningRepository;
        _speakingRepository = speakingRepository;
        _writingQuestionRepository = writingQuestionRepository;
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
            ExamType = entity.ExamType,
            QuestionIds = entity.QuestionIds,
            Attempts = entity.Attempts
        };
    }

    public async Task<ExamDto> CreateExamAsync(SaveExamDto dto)
    {
        // 1. Generate clean exam id from Title (e.g. slugified)
        var examId = string.IsNullOrEmpty(dto.Title) 
             ? Guid.NewGuid().ToString() 
             : dto.Title.ToLower()
                 .Replace(" ", "_")
                 .Replace("-", "_")
                 .Replace("#", "")
                 .Replace("—", "_");
        
        // Ensure slug is clean
        examId = string.Concat(examId.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_'))
            .Trim('_')
            .ToLower();

        if (string.IsNullOrEmpty(examId)) examId = Guid.NewGuid().ToString();

        // Append type suffix if not full
        if (!string.IsNullOrEmpty(dto.ExamType) && dto.ExamType != "full")
        {
            examId = $"{examId}_{dto.ExamType.ToLower().Trim()}";
        }

        // 2. Add question groups
        foreach (var groupDto in dto.QuestionGroups)
        {
            var group = new QuestionGroup
            {
                Id = groupDto.Id,
                Part = groupDto.Part,
                PassageText = groupDto.PassageText,
                Script = groupDto.Script,
                ImageUrl = groupDto.ImageUrl,
                AudioUrl = groupDto.AudioUrl,
                QuestionIds = groupDto.QuestionIds,
                QuestionCount = groupDto.QuestionCount,
                Source = groupDto.Source ?? dto.Title,
                CreatedAt = DateTime.UtcNow
            };
            await _listeningRepository.AddGroupAsync(group);
        }

        // 3. Add questions
        var questionIds = new List<string>();
        foreach (var qDto in dto.Questions)
        {
            var question = new ListeningQuestion
            {
                Id = qDto.Id,
                Part = qDto.Part,
                QuestionText = qDto.QuestionText,
                ImageUrl = qDto.ImageUrl,
                AudioUrl = qDto.AudioUrl,
                Options = qDto.Options,
                CorrectAnswer = qDto.CorrectAnswer,
                Explanation = qDto.Explanation,
                ExplanationVi = qDto.ExplanationVi,
                Script = qDto.Script,
                GroupId = qDto.GroupId,
                Difficulty = qDto.Difficulty,
                Skill = qDto.Skill,
                CreatedAt = DateTime.UtcNow,
                IsForExam = true,
                IsForPractice = false,
                ExamId = examId
            };
            await _listeningRepository.AddQuestionAsync(question);
            questionIds.Add(qDto.Id);
        }

        // 4. Add Exam record
        var exam = new Exam
        {
            Id = examId,
            Title = dto.Title,
            Description = dto.Description,
            Difficulty = dto.Difficulty,
            Duration = dto.Duration,
            Year = dto.Year,
            ImageUrl = dto.ImageUrl,
            AudioUrl = dto.AudioUrl,
            IsExam = dto.IsExam,
            IsPractice = dto.IsPractice,
            IsPremium = dto.IsPremium,
            IsPublished = dto.IsPublished,
            ExamType = dto.ExamType ?? "full",
            QuestionIds = questionIds
        };

        await _repository.AddExamAsync(exam);

        return MapToDto(exam);
    }

    public async Task<ExamDto?> UpdateExamAsync(string id, UpdateExamDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        // Merge: only update non-null fields from DTO
        var exam = new Exam
        {
            Id = id,
            Title       = dto.Title       ?? existing.Title,
            Description = dto.Description ?? existing.Description,
            Difficulty  = dto.Difficulty  ?? existing.Difficulty,
            Duration    = dto.Duration    ?? existing.Duration,
            Year        = dto.Year        ?? existing.Year,
            ImageUrl    = dto.ImageUrl    ?? existing.ImageUrl,
            AudioUrl    = dto.AudioUrl    ?? existing.AudioUrl,
            ExamType    = dto.ExamType    ?? existing.ExamType,
            IsExam      = dto.IsExam      ?? existing.IsExam,
            IsPractice  = dto.IsPractice  ?? existing.IsPractice,
            IsPremium   = dto.IsPremium   ?? existing.IsPremium,
            IsPublished = dto.IsPublished ?? existing.IsPublished,
            QuestionIds = existing.QuestionIds
        };

        var success = await _repository.UpdateExamAsync(exam);
        return success ? MapToDto(exam) : null;
    }

    public async Task<bool> DeleteExamAsync(string id)
    {
        // 1. Delete questions associated with the exam (L&R)
        var questions = await _repository.GetExamQuestionsAsync(id);
        foreach (var q in questions)
        {
            await _listeningRepository.DeleteQuestionAsync(q.Id);
        }

        // 2. Delete speaking questions
        var speakingQuestions = await _speakingRepository.GetByExamSetIdAsync(id);
        foreach (var sq in speakingQuestions)
        {
            await _speakingRepository.DeleteAsync(sq.Id);
        }

        // 3. Delete writing questions
        var writingQuestions = await _writingQuestionRepository.GetQuestionsByExamSetIdAsync(id);
        foreach (var wq in writingQuestions)
        {
            await _writingQuestionRepository.DeleteAsync(wq.Id);
        }

        // 4. Delete the exam itself
        return await _repository.DeleteExamAsync(id);
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
