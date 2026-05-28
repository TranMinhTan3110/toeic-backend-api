using Google.Cloud.Firestore;

namespace ToeicBackend.Domain.Entities;

/// <summary>
/// Một bài viết nộp (1 câu hỏi Writing) — collection <c>writing_submissions</c>.
/// Theo hướng dẫn history_implementation_guide: mỗi câu viết được lưu thành 1 Document riêng biệt.
/// </summary>
[FirestoreData]
public class WritingHistory
{
    [FirestoreDocumentId]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("user_id")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("question_id")]
    public string QuestionId { get; set; } = string.Empty;

    [FirestoreProperty("task_number")]
    public int? TaskNumber { get; set; }

    [FirestoreProperty("task_type")]
    public string? TaskType { get; set; }

    [FirestoreProperty("session_type")]
    public string SessionType { get; set; } = "practice"; // "practice" | "exam"

    [FirestoreProperty("user_answer")]
    public string UserAnswer { get; set; } = string.Empty;

    [FirestoreProperty("word_count")]
    public int? WordCount { get; set; }

    [FirestoreProperty("time_used")]
    public int? TimeUsed { get; set; } // giây

    [FirestoreProperty("ai_score")]
    public int? AiScore { get; set; }

    [FirestoreProperty("ai_feedback")]
    public WritingAiFeedback? AiFeedback { get; set; }

    [FirestoreProperty("submitted_at")]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    [FirestoreProperty("question_ids")]
    public List<string>? QuestionIds { get; set; }

    [FirestoreProperty("answers")]
    public Dictionary<string, string>? Answers { get; set; }

    [FirestoreProperty("question_count")]
    public int? QuestionCount { get; set; }

    [FirestoreProperty("correct_count")]
    public int? CorrectCount { get; set; }

    [FirestoreProperty("time_spent")]
    public int? TimeSpent { get; set; }

    [FirestoreProperty("incorrect_ids")]
    public List<string>? IncorrectIds { get; set; }
}
