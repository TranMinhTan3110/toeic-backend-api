using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace ToeicBackend.Domain.Entities;

[FirestoreData]
public class FullTestHistory
{
    [FirestoreDocumentId]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("user_id")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("exam_id")]
    public string ExamId { get; set; } = string.Empty;

    [FirestoreProperty("exam_title")]
    public string ExamTitle { get; set; } = string.Empty;

    [FirestoreProperty("score_listening")]
    public int ScoreListening { get; set; }

    [FirestoreProperty("score_reading")]
    public int ScoreReading { get; set; }

    [FirestoreProperty("total_score")]
    public int TotalScore { get; set; }

    [FirestoreProperty("correct_count")]
    public int CorrectCount { get; set; }

    [FirestoreProperty("total_count")]
    public int TotalCount { get; set; }

    [FirestoreProperty("time_spent")]
    public int TimeSpent { get; set; } // in seconds

    [FirestoreProperty("completed_at")]
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    [FirestoreProperty("answers")]
    public Dictionary<string, string> Answers { get; set; } = new();

    [FirestoreProperty("part_scores")]
    public Dictionary<string, int> PartScores { get; set; } = new();
}
