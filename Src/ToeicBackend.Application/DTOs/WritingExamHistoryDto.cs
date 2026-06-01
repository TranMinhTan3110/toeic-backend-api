namespace ToeicBackend.Application.DTOs;

/// <summary>Kết quả 1 lần thi Writing full exam.</summary>
public class WritingExamHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ExamSetId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    /// <summary>Điểm TOEIC Writing 0-200 (scale từ raw AI scores).</summary>
    public double ToeicScore { get; set; }
    /// <summary>Điểm trung bình raw AI (0-10).</summary>
    public double RawAverageScore { get; set; }
    public int TotalTasks { get; set; }
    /// <summary>Thời gian làm bài (giây).</summary>
    public int TimeSpent { get; set; }
    public DateTime Date { get; set; }
    public List<WritingExamTaskResultDto> TaskResults { get; set; } = new();
    public int EpAwarded { get; set; }
}

public class WritingExamTaskResultDto
{
    public string QuestionId { get; set; } = string.Empty;
    public int TaskNumber { get; set; }
    public string TaskType { get; set; } = string.Empty;   // "sentence" | "email" | "essay"
    public string UserAnswer { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public int AiScore { get; set; }        // 0-10
    public string AiFeedback { get; set; } = string.Empty;
    public Dictionary<string, double> CriteriaScores { get; set; } = new();
}

/// <summary>Request gửi lên sau khi user nộp toàn bộ bài thi Writing.</summary>
public class SaveWritingExamRequestDto
{
    public string ExamSetId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public int TimeSpent { get; set; }
    public List<WritingExamTaskSubmitDto> Tasks { get; set; } = new();
}

public class WritingExamTaskSubmitDto
{
    public string QuestionId { get; set; } = string.Empty;
    public int TaskNumber { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
    public int WordCount { get; set; }
}
