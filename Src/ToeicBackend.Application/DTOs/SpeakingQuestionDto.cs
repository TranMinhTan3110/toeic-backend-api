namespace ToeicBackend.Application.DTOs;

public class SpeakingQuestionDto
{
    public string Id { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public int TaskNumber { get; set; }
    public string PromptText { get; set; } = string.Empty;
    public string? PromptImageUrl { get; set; }
    public string? PromptAudioUrl { get; set; }
    public int PreparationTime { get; set; }
    public int ResponseTime { get; set; }
    public string Difficulty { get; set; } = "medium";
    public string AiPrompt { get; set; } = string.Empty;
    public List<string> ScoringCriteria { get; set; } = new();
    public string? ExamSetId { get; set; }
    public string? Topic { get; set; }
    public bool IsPractice { get; set; }
    public int MaxScore { get; set; }
    public string? SampleAnswer { get; set; }
    public List<string> Questions { get; set; } = new();
    public List<int> AnswerTimes { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
}
