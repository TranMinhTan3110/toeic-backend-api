using Google.Cloud.Firestore;

namespace ToeicBackend.Domain.Entities;

[FirestoreData]
public class SpeakingHistoryAnswer
{
    [FirestoreProperty("question_id")]
    public string QuestionId { get; set; } = string.Empty;

    [FirestoreProperty("sub_question_index")]
    public int? SubQuestionIndex { get; set; }

    [FirestoreProperty("transcript")]
    public string Transcript { get; set; } = string.Empty;

    [FirestoreProperty("audio_url")]
    public string AudioUrl { get; set; } = string.Empty;

    [FirestoreProperty("overall_score")]
    public double OverallScore { get; set; }

    [FirestoreProperty("passed")]
    public bool Passed { get; set; }

    [FirestoreProperty("feedback")]
    public string Feedback { get; set; } = string.Empty;

    [FirestoreProperty("criteria_scores")]
    public Dictionary<string, double> CriteriaScores { get; set; } = new();

    [FirestoreProperty("ai_feedback")]
    public SpeakingAiFeedback AiFeedback { get; set; } = new();
}
