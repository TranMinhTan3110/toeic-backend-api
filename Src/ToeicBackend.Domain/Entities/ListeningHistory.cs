using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace ToeicBackend.Domain.Entities;

[FirestoreData]
public class ListeningHistory
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

    [FirestoreProperty("date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [FirestoreProperty("incorrect_question_ids")]
    public List<string> IncorrectQuestionIds { get; set; } = new();

    [FirestoreProperty("selected_answers")]
    public Dictionary<string, string> SelectedAnswers { get; set; } = new();
}
