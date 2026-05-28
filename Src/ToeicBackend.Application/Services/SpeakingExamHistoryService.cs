using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Enums;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.Application.Services;

public class SpeakingExamHistoryService : ISpeakingExamHistoryService
{
    private readonly ISpeakingExamHistoryRepository _repository;
    private readonly ISpeakingRepository _speakingRepository;
    private readonly IAiService _aiService;
    private readonly IEngagementService _engagementService;

    public SpeakingExamHistoryService(
        ISpeakingExamHistoryRepository repository,
        ISpeakingRepository speakingRepository,
        IAiService aiService,
        IEngagementService engagementService)
    {
        _repository = repository;
        _speakingRepository = speakingRepository;
        _aiService = aiService;
        _engagementService = engagementService;
    }

    public async Task<SpeakingExamHistoryDto> SubmitExamAsync(string userId, SaveSpeakingExamRequestDto request)
    {
        var taskResults = new List<SpeakingExamTaskResultDto>();

        // Chấm điểm batch — mỗi task gọi AI song song
        var evalTasks = request.Tasks.Select(async task =>
        {
            var question = await _speakingRepository.GetByIdAsync(task.QuestionId);
            if (question == null) return new SpeakingExamTaskResultDto
            {
                QuestionId = task.QuestionId,
                SubQuestionIndex = task.SubQuestionIndex,
                Transcript = task.Transcript ?? "",
                Score = 0,
                Feedback = "Không tìm thấy câu hỏi",
                Passed = false
            };

            // Parse audio nếu có
            byte[]? audioBytes = null;
            string? mimeType = null;
            if (!string.IsNullOrEmpty(task.AudioBase64) && !string.IsNullOrEmpty(task.AudioMimeType))
            {
                try
                {
                    audioBytes = Convert.FromBase64String(task.AudioBase64);
                    mimeType = task.AudioMimeType;
                }
                catch { /* bỏ qua nếu base64 lỗi */ }
            }

            // Lấy prompt + sampleAnswers từ question
            var promptText = question.PromptText ?? "";
            if (task.SubQuestionIndex.HasValue && question.Questions != null &&
                task.SubQuestionIndex.Value < question.Questions.Count)
            {
                promptText = question.Questions[task.SubQuestionIndex.Value];
            }

            var sampleAnswers = question.SampleAnswer != null
                ? new[] { question.SampleAnswer }
                : Array.Empty<string>();

            var eval = await _aiService.EvaluateSpeakingAsync(
                taskPrompt: promptText,
                sampleAnswers: sampleAnswers,
                userTranscript: task.Transcript ?? "",
                taskNumber: question.TaskNumber,
                audioBytes: audioBytes,
                mimeType: mimeType);

            return new SpeakingExamTaskResultDto
            {
                QuestionId = task.QuestionId,
                SubQuestionIndex = task.SubQuestionIndex,
                Transcript = task.Transcript ?? "",
                Score = eval.OverallScore,
                Feedback = eval.Feedback,
                CriteriaScores = eval.CriteriaScores,
                Passed = eval.Passed
            };
        });

        taskResults = (await Task.WhenAll(evalTasks)).ToList();

        // Tính điểm TOEIC Speaking 0-200
        var rawAvg = taskResults.Count > 0 ? taskResults.Average(t => t.Score) : 0;
        // AI cho điểm 0-5 → scale lên 0-200
        var toeicScore = Math.Round(rawAvg / 5.0 * 200, 0);

        var historyDto = new SpeakingExamHistoryDto
        {
            UserId = userId,
            ExamSetId = request.ExamSetId,
            ExamTitle = request.ExamTitle,
            ToeicScore = toeicScore,
            RawAverageScore = Math.Round(rawAvg, 2),
            TotalTasks = taskResults.Count,
            Date = DateTime.UtcNow,
            TaskResults = taskResults
        };

        var savedId = await _repository.AddAsync(historyDto);
        historyDto.Id = savedId;

        // Ghi nhận EP gamification (+30 EP vĩnh viễn cho đề thi này)
        try
        {
            await _engagementService.RecordActivityAsync(userId, new RecordActivityRequest
            {
                ActivityType = ActivityType.SpeakingExamComplete,
                ReferenceId = request.ExamSetId
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SpeakingExam] Lỗi cộng EP: {ex.Message}");
        }

        return historyDto;
    }

    public async Task<IEnumerable<SpeakingExamHistoryDto>> GetUserHistoryAsync(string userId)
        => await _repository.GetByUserIdAsync(userId);

    public async Task<SpeakingExamHistoryDto?> GetByIdAsync(string id)
        => await _repository.GetByIdAsync(id);
}
