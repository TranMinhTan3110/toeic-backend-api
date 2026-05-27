using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace ToeicBackend.Domain.Entities;

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

    [FirestoreProperty("question_ids")]
    public List<string> QuestionIds { get; set; } = new();

    [FirestoreProperty("question_count")]
    public int QuestionCount { get; set; } = 1;

    [FirestoreProperty("answers")]
    public Dictionary<string, string> Answers { get; set; } = new();

    [FirestoreProperty("session_type")]
    public string SessionType { get; set; } = "practice";

    [FirestoreProperty("user_answer")]
    public string UserAnswer { get; set; } = string.Empty;

    [FirestoreProperty("word_count")]
    public int? WordCount { get; set; }

    [FirestoreProperty("time_used")]
    public int? TimeUsed { get; set; }

    [FirestoreProperty("ai_score")]
    public int? AiScore { get; set; }

    [FirestoreProperty("ai_feedback")]
    public WritingAiFeedback? AiFeedback { get; set; }

    [FirestoreProperty("ai_model")]
    public string? AiModel { get; set; }

    [FirestoreProperty("status")]
    public string Status { get; set; } = "scored";

    [FirestoreProperty("scored_at")]
    public DateTime? ScoredAt { get; set; }

    [FirestoreProperty("result_id")]
    public string? ResultId { get; set; }

    [FirestoreProperty("submitted_at")]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
