using Google.Cloud.Firestore;

namespace ToeicBackend.Domain.Entities;

/// <summary>
/// Một phiên luyện nói (nhiều câu) — collection <c>user_speaking_history</c>.
/// </summary>
[FirestoreData]
public class SpeakingHistory
{
    [FirestoreDocumentId]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("user_id")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("part")]
    public int Part { get; set; }

    [FirestoreProperty("correct_count")]
    public int CorrectCount { get; set; }

    [FirestoreProperty("total_count")]
    public int TotalCount { get; set; }

    [FirestoreProperty("percent")]
    public double Percent { get; set; }

    [FirestoreProperty("score")]
    public double Score { get; set; }

    [FirestoreProperty("feedback_summary")]
    public string FeedbackSummary { get; set; } = string.Empty;

    [FirestoreProperty("criteria")]
    public Dictionary<string, double> Criteria { get; set; } = new();

    [FirestoreProperty("session_type")]
    public string SessionType { get; set; } = "practice";

    [FirestoreProperty("date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [FirestoreProperty("answers")]
    public List<SpeakingHistoryAnswer> Answers { get; set; } = new();
}
