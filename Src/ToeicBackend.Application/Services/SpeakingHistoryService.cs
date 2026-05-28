using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class SpeakingHistoryService : ISpeakingHistoryService
{
    private const double PassThreshold = 6.0;

    private readonly ISpeakingHistoryRepository _historyRepository;
    private readonly ISpeakingRepository _questionRepository;
    private readonly IAiService _aiService;

    public SpeakingHistoryService(
        ISpeakingHistoryRepository historyRepository,
        ISpeakingRepository questionRepository,
        IAiService aiService)
    {
        _historyRepository = historyRepository;
        _questionRepository = questionRepository;
        _aiService = aiService;
    }

    public async Task<string> SaveHistoryAsync(string userId, SaveSpeakingHistoryRequestDto request)
    {
        var entity = new SpeakingHistory
        {
            UserId = userId,
            Part = request.Part,
            CorrectCount = request.CorrectCount,
            TotalCount = request.TotalCount,
            Percent = request.Percent,
            Score = request.Score,
            FeedbackSummary = request.FeedbackSummary,
            Criteria = request.Criteria ?? new Dictionary<string, double>(),
            SessionType = string.IsNullOrWhiteSpace(request.SessionType) ? "practice" : request.SessionType,
            Date = DateTime.UtcNow,
            Answers = request.Answers.Select(a => new SpeakingHistoryAnswer
            {
                QuestionId = a.QuestionId,
                SubQuestionIndex = a.SubQuestionIndex,
                Transcript = a.Transcript,
                AudioUrl = a.AudioUrl,
                OverallScore = a.OverallScore,
                Passed = a.Passed,
                Feedback = a.Feedback,
                CriteriaScores = a.CriteriaScores ?? new Dictionary<string, double>(),
                AiFeedback = MapAiFeedback(a.AiFeedback, a.OverallScore)
            }).ToList()
        };

        return await _historyRepository.AddAsync(entity);
    }

    public async Task<IEnumerable<SpeakingHistoryDto>> GetUserHistoryAsync(string userId, string? sessionType = null)
    {
        var entities = await _historyRepository.GetByUserIdAsync(userId, sessionType);
        return entities.Select(MapToDto);
    }

    public async Task<SpeakingHistoryDto?> GetHistoryByIdAsync(string id)
    {
        var entity = await _historyRepository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<SpeakingEvaluationDto> EvaluateAsync(
        string questionId,
        string transcript,
        int? subQuestionIndex,
        byte[]? audioBytes = null,
        string? mimeType = null)
    {
        var question = await _questionRepository.GetByIdAsync(questionId)
            ?? throw new InvalidOperationException("Không tìm thấy câu hỏi Speaking.");

        var prompt = BuildTaskPrompt(question, subQuestionIndex);
        var samples = CollectSampleAnswers(question, subQuestionIndex);

        if (string.IsNullOrWhiteSpace(transcript))
        {
            return new SpeakingEvaluationDto
            {
                OverallScore = 0,
                Passed = false,
                Feedback = "Không nhận diện được giọng nói. Hãy nói rõ hơn và thử lại.",
                Transcript = string.Empty
            };
        }

        return await _aiService.EvaluateSpeakingAsync(
            prompt,
            samples,
            transcript.Trim(),
            question.TaskNumber,
            audioBytes,
            mimeType);
    }

    private static string BuildTaskPrompt(SpeakingQuestion question, int? subQuestionIndex)
    {
        if (subQuestionIndex is >= 0 &&
            question.Questions.Count > subQuestionIndex.Value)
        {
            return $"{question.PromptText}\n\nCâu hỏi phụ: {question.Questions[subQuestionIndex.Value]}";
        }

        return question.PromptText;
    }

    private static List<string> CollectSampleAnswers(SpeakingQuestion question, int? subQuestionIndex)
    {
        var samples = new List<string>();

        if (!string.IsNullOrWhiteSpace(question.SampleAnswer))
        {
            samples.Add(question.SampleAnswer.Trim());
        }

        var explanation = question.Explanation;
        if (explanation != null)
        {
            if (explanation.SampleAnswers.Count > 0)
            {
                if (subQuestionIndex is >= 0 && subQuestionIndex.Value < explanation.SampleAnswers.Count)
                {
                    samples.Add(explanation.SampleAnswers[subQuestionIndex.Value]);
                }
                else
                {
                    samples.AddRange(explanation.SampleAnswers);
                }
            }
        }

        return samples
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();
    }

    private static SpeakingAiFeedback MapAiFeedback(SpeakingSubmissionAiFeedbackDto dto, double overallScore)
    {
        var scale = overallScore / 2.0; // map 0-10 → 0-5 cho Firestore
        return new SpeakingAiFeedback
        {
            Pronunciation = dto.Pronunciation > 0 ? dto.Pronunciation : (int)Math.Round(scale),
            Grammar = dto.Grammar > 0 ? dto.Grammar : (int)Math.Round(scale),
            Vocabulary = dto.Vocabulary > 0 ? dto.Vocabulary : (int)Math.Round(scale),
            FeedbackVi = string.IsNullOrWhiteSpace(dto.FeedbackVi) ? string.Empty : dto.FeedbackVi
        };
    }

    private static SpeakingHistoryDto MapToDto(SpeakingHistory entity)
    {
        return new SpeakingHistoryDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Part = entity.Part,
            CorrectCount = entity.CorrectCount,
            TotalCount = entity.TotalCount,
            Percent = entity.Percent,
            Score = entity.Score,
            FeedbackSummary = entity.FeedbackSummary,
            Criteria = entity.Criteria ?? new Dictionary<string, double>(),
            SessionType = entity.SessionType,
            Date = entity.Date,
            Answers = entity.Answers.Select(a => new SpeakingHistoryAnswerDto
            {
                QuestionId = a.QuestionId,
                SubQuestionIndex = a.SubQuestionIndex,
                Transcript = a.Transcript,
                AudioUrl = a.AudioUrl,
                OverallScore = a.OverallScore,
                Passed = a.Passed,
                Feedback = a.Feedback,
                CriteriaScores = a.CriteriaScores ?? new Dictionary<string, double>(),
                AiFeedback = new SpeakingSubmissionAiFeedbackDto
                {
                    Pronunciation = a.AiFeedback.Pronunciation,
                    Grammar = a.AiFeedback.Grammar,
                    Vocabulary = a.AiFeedback.Vocabulary,
                    FeedbackVi = a.AiFeedback.FeedbackVi
                }
            }).ToList()
        };
    }
}
