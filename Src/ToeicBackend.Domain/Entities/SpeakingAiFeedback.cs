using Google.Cloud.Firestore;

namespace ToeicBackend.Domain.Entities;

[FirestoreData]
public class SpeakingAiFeedback
{
    [FirestoreProperty("pronunciation")]
    public int Pronunciation { get; set; }

    [FirestoreProperty("grammar")]
    public int Grammar { get; set; }

    [FirestoreProperty("vocabulary")]
    public int Vocabulary { get; set; }

    [FirestoreProperty("feedback_vi")]
    public string FeedbackVi { get; set; } = string.Empty;
}
