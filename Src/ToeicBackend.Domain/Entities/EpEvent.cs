using Google.Cloud.Firestore;

namespace ToeicBackend.Domain.Entities;

[FirestoreData]
public class EpEvent
{
    [FirestoreDocumentId]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty]
    public string ActivityType { get; set; } = string.Empty;

    [FirestoreProperty]
    public int BaseEp { get; set; }

    [FirestoreProperty]
    public double StreakMultiplier { get; set; }

    [FirestoreProperty]
    public int EpAwarded { get; set; }

    [FirestoreProperty]
    public string? ReferenceId { get; set; }

    [FirestoreProperty]
    public string StudyDateKey { get; set; } = string.Empty;

    [FirestoreProperty]
    public DateTime CreatedAt { get; set; }
}
