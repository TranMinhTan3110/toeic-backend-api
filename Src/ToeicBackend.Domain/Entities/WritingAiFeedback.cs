using Google.Cloud.Firestore;

namespace ToeicBackend.Domain.Entities;

[FirestoreData]
public class WritingAiFeedback
{
    [FirestoreProperty("grammar_score")]
    public int? GrammarScore { get; set; }

    [FirestoreProperty("vocabulary_score")]
    public int? VocabularyScore { get; set; }

    [FirestoreProperty("cohesion_score")]
    public int? CohesionScore { get; set; }

    [FirestoreProperty("corrections_vi")]
    public string? CorrectionsVi { get; set; }

    [FirestoreProperty("suggested_improvement")]
    public string? SuggestedImprovement { get; set; }
}
