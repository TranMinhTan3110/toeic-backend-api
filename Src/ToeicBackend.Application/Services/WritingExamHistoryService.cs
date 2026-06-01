using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Enums;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.Application.Services;

public class WritingExamHistoryService : IWritingExamHistoryService
{
    private readonly IWritingExamHistoryRepository _repository;
    private readonly IWritingQuestionRepository _questionRepository;
    private readonly IAiService _aiService;
    private readonly IEngagementService _engagementService;

    public WritingExamHistoryService(
        IWritingExamHistoryRepository repository,
        IWritingQuestionRepository questionRepository,
        IAiService aiService,
        IEngagementService engagementService)
    {
        _repository = repository;
        _questionRepository = questionRepository;
        _aiService = aiService;
        _engagementService = engagementService;
    }

    public async Task<WritingExamHistoryDto> SubmitExamAsync(string userId, SaveWritingExamRequestDto request)
    {
        // Chấm điểm batch — mỗi task gọi AI song song
        var evalTasks = request.Tasks.Select(async task =>
        {
            var question = await _questionRepository.GetByIdAsync(task.QuestionId);
            if (question == null) return new WritingExamTaskResultDto
            {
                QuestionId = task.QuestionId,
                TaskNumber = task.TaskNumber,
                TaskType = task.TaskType,
                UserAnswer = task.UserAnswer,
                WordCount = task.WordCount,
                AiScore = 0,
                AiFeedback = "Không tìm thấy câu hỏi"
            };

            if (string.IsNullOrWhiteSpace(task.UserAnswer))
            {
                return new WritingExamTaskResultDto
                {
                    QuestionId = task.QuestionId,
                    TaskNumber = task.TaskNumber,
                    TaskType = task.TaskType,
                    UserAnswer = "",
                    WordCount = 0,
                    AiScore = 0,
                    AiFeedback = "Bạn chưa trả lời câu này."
                };
            }

            var givenWords = question.GivenWords ?? new List<string>();
            var emailContent = question.EmailContent;
            var emailQuestions = question.EmailQuestions ?? new List<string>();
            var samples = new List<string>();
            if (!string.IsNullOrWhiteSpace(question.SampleAnswer))
                samples.Add(question.SampleAnswer.Trim());

            var eval = await _aiService.EvaluateWritingAsync(
                taskPrompt: question.PromptText,
                taskType: question.TaskType,
                givenWords: givenWords,
                emailContent: emailContent,
                emailQuestions: emailQuestions,
                sampleAnswers: samples,
                userAnswer: task.UserAnswer.Trim());

            var criteriaScores = new Dictionary<string, double>();
            if (eval.CriteriaScores != null)
                criteriaScores = eval.CriteriaScores;

            return new WritingExamTaskResultDto
            {
                QuestionId = task.QuestionId,
                TaskNumber = task.TaskNumber,
                TaskType = task.TaskType,
                UserAnswer = task.UserAnswer,
                WordCount = task.WordCount,
                AiScore = (int)Math.Round(eval.OverallScore),
                AiFeedback = eval.Feedback ?? "",
                CriteriaScores = criteriaScores
            };
        });

        var taskResults = (await Task.WhenAll(evalTasks)).ToList();

        // Tính điểm TOEIC Writing 0-200
        // AI cho điểm 0-10 → scale lên 0-200
        var rawAvg = taskResults.Count > 0 ? taskResults.Average(t => t.AiScore) : 0;
        var toeicScore = Math.Round(rawAvg / 10.0 * 200, 0);

        var historyDto = new WritingExamHistoryDto
        {
            UserId = userId,
            ExamSetId = request.ExamSetId,
            ExamTitle = request.ExamTitle,
            ToeicScore = toeicScore,
            RawAverageScore = Math.Round(rawAvg, 2),
            TotalTasks = taskResults.Count,
            TimeSpent = request.TimeSpent,
            Date = DateTime.UtcNow,
            TaskResults = taskResults
        };

        var savedId = await _repository.AddAsync(historyDto);
        historyDto.Id = savedId;

        // Ghi nhận EP gamification (+30 EP, chỉ lần đầu hoàn thành đề này)
        try
        {
            var epResult = await _engagementService.RecordActivityAsync(userId, new RecordActivityRequest
            {
                ActivityType = ActivityType.WritingExamComplete,
                ReferenceId = request.ExamSetId
            });
            // Gán EP thực tế trả về để Frontend hiển thị đúng (0 nếu đã làm rồi)
            historyDto.EpAwarded = epResult?.EpAwarded ?? 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WritingExam] Lỗi cộng EP: {ex.Message}");
        }

        return historyDto;
    }

    public async Task<IEnumerable<WritingExamHistoryDto>> GetUserHistoryAsync(string userId)
        => await _repository.GetByUserIdAsync(userId);

    public async Task<WritingExamHistoryDto?> GetByIdAsync(string id)
        => await _repository.GetByIdAsync(id);
}
