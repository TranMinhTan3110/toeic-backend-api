namespace ToeicBackend.Application.DTOs;

public class SpeakingHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int Part { get; set; }
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public double Percent { get; set; }
    public double Score { get; set; }
    public string FeedbackSummary { get; set; } = string.Empty;
    public Dictionary<string, double> Criteria { get; set; } = new();
    public string SessionType { get; set; } = "practice";
    public DateTime Date { get; set; }
    public List<SpeakingHistoryAnswerDto> Answers { get; set; } = new();
}

public class SpeakingHistoryAnswerDto
{
    public string QuestionId { get; set; } = string.Empty;
    public int? SubQuestionIndex { get; set; }
    public string Transcript { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public double OverallScore { get; set; }
    public bool Passed { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public Dictionary<string, double> CriteriaScores { get; set; } = new();
    public SpeakingSubmissionAiFeedbackDto AiFeedback { get; set; } = new();
}

public class SaveSpeakingHistoryRequestDto
{
    public int Part { get; set; }
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public double Percent { get; set; }
    public double Score { get; set; }
    public string FeedbackSummary { get; set; } = string.Empty;
    public Dictionary<string, double> Criteria { get; set; } = new();
    public string SessionType { get; set; } = "practice";
    public List<SaveSpeakingHistoryAnswerDto> Answers { get; set; } = new();
}

public class SaveSpeakingHistoryAnswerDto
{
    public string QuestionId { get; set; } = string.Empty;
    public int? SubQuestionIndex { get; set; }
    public string Transcript { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public double OverallScore { get; set; }
    public bool Passed { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public Dictionary<string, double> CriteriaScores { get; set; } = new();
    public SpeakingSubmissionAiFeedbackDto AiFeedback { get; set; } = new();
}

public class SpeakingEvaluationDto
{
    public double OverallScore { get; set; }
    public Dictionary<string, double> CriteriaScores { get; set; } = new();
    public string Feedback { get; set; } = string.Empty;
    public string? Transcript { get; set; }
    public bool Passed { get; set; }
}
