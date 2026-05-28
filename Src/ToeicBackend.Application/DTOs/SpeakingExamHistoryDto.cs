namespace ToeicBackend.Application.DTOs;

/// <summary>Kết quả 1 lần thi Speaking full exam.</summary>
public class SpeakingExamHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ExamSetId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    /// <summary>Điểm TOEIC Speaking 0-200 (scale từ raw AI scores).</summary>
    public double ToeicScore { get; set; }
    /// <summary>Điểm trung bình raw AI (0-5).</summary>
    public double RawAverageScore { get; set; }
    public int TotalTasks { get; set; }
    public DateTime Date { get; set; }
    public List<SpeakingExamTaskResultDto> TaskResults { get; set; } = new();
}

public class SpeakingExamTaskResultDto
{
    public string QuestionId { get; set; } = string.Empty;
    public int? SubQuestionIndex { get; set; }
    public string Transcript { get; set; } = string.Empty;
    public double Score { get; set; }       // 0-5
    public string Feedback { get; set; } = string.Empty;
    public Dictionary<string, double> CriteriaScores { get; set; } = new();
    public bool Passed { get; set; }
}

/// <summary>Request gửi lên sau khi user nộp toàn bộ bài thi Speaking.</summary>
public class SaveSpeakingExamRequestDto
{
    public string ExamSetId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    /// <summary>Danh sách transcript + audio của từng task, theo thứ tự.</summary>
    public List<SpeakingExamTaskSubmitDto> Tasks { get; set; } = new();
}

public class SpeakingExamTaskSubmitDto
{
    public string QuestionId { get; set; } = string.Empty;
    public int? SubQuestionIndex { get; set; }
    public string? Transcript { get; set; }
    public string? AudioBase64 { get; set; }  // base64 audio hoặc null
    public string? AudioMimeType { get; set; }
}
