namespace ToeicBackend.Application.DTOs;

public class WritingHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public int? TaskNumber { get; set; }
    public string? TaskType { get; set; }
    public string SessionType { get; set; } = "practice";
    public string UserAnswer { get; set; } = string.Empty;
    public int? WordCount { get; set; }
    public int? TimeUsed { get; set; }
    public int? AiScore { get; set; }
    public WritingHistoryAiFeedbackDto? AiFeedback { get; set; }
    public DateTime SubmittedAt { get; set; }
    public List<string>? QuestionIds { get; set; }
    public Dictionary<string, string>? Answers { get; set; }
    public int? QuestionCount { get; set; }
    public int? CorrectCount { get; set; }
    public int? TimeSpent { get; set; }
    public List<string>? IncorrectIds { get; set; }
}

public class SaveWritingHistoryRequestDto
{
    public string QuestionId { get; set; } = string.Empty;
    public int? TaskNumber { get; set; }
    public string? TaskType { get; set; }
    public string SessionType { get; set; } = "practice";
    public string UserAnswer { get; set; } = string.Empty;
    public int? WordCount { get; set; }
    public int? TimeUsed { get; set; }
    public int? AiScore { get; set; }
    public WritingHistoryAiFeedbackDto? AiFeedback { get; set; }
}

public class SaveWritingSessionRequestDto
{
    public string? Id { get; set; }
    public List<string>? QuestionIds { get; set; }
    public Dictionary<string, string>? Answers { get; set; }
    public int? TaskNumber { get; set; }
    public string? TaskType { get; set; }
    public string SessionType { get; set; } = "practice";
    public int? QuestionCount { get; set; }
    public int? CorrectCount { get; set; }
    public int? TimeSpent { get; set; }
    public List<string>? IncorrectIds { get; set; }
}
