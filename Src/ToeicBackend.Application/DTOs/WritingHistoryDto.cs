namespace ToeicBackend.Application.DTOs;

public class WritingHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public int? TaskNumber { get; set; }
    public string? TaskType { get; set; }
    public List<string> QuestionIds { get; set; } = new();
    public int QuestionCount { get; set; } = 1;
    public Dictionary<string, string> Answers { get; set; } = new();
    public string SessionType { get; set; } = "practice";
    public string UserAnswer { get; set; } = string.Empty;
    public int? WordCount { get; set; }
    public int? TimeUsed { get; set; }
    public int? AiScore { get; set; }
    public WritingHistoryAiFeedbackDto? AiFeedback { get; set; }
    public string? AiModel { get; set; }
    public string Status { get; set; } = "scored";
    public DateTime? ScoredAt { get; set; }
    public string? ResultId { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class SaveWritingHistoryRequestDto
{
    public string QuestionId { get; set; } = string.Empty;
    public int? TaskNumber { get; set; }
    public string? TaskType { get; set; }
    public List<string>? QuestionIds { get; set; }
    public int? QuestionCount { get; set; }
    public Dictionary<string, string>? Answers { get; set; }
    public string SessionType { get; set; } = "practice";
    public string UserAnswer { get; set; } = string.Empty;
    public int? WordCount { get; set; }
    public int? TimeUsed { get; set; }
    public int? AiScore { get; set; }
    public WritingHistoryAiFeedbackDto? AiFeedback { get; set; }
    public string? AiModel { get; set; }
    public string? Status { get; set; }
    public DateTime? ScoredAt { get; set; }
    public string? ResultId { get; set; }
}
