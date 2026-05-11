using Google.Cloud.Firestore;

namespace ToeicBackend.Domain.Entities;

[FirestoreData]
public class User
{
    [FirestoreDocumentId]
    public string Uid { get; set; } = string.Empty;

    [FirestoreProperty]
    public string DisplayName { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Email { get; set; } = string.Empty;

    [FirestoreProperty]
    public string? AvatarUrl { get; set; }

    [FirestoreProperty]
    public DateTime CreatedAt { get; set; }

    [FirestoreProperty]
    public int TargetScore { get; set; }

    [FirestoreProperty]
    public string CurrentLevel { get; set; } = "beginner";

    [FirestoreProperty]
    public string Plan { get; set; } = "free";

    [FirestoreProperty]
    public int StreakDays { get; set; }

    [FirestoreProperty]
    public int ExperiencePoints { get; set; }

    [FirestoreProperty]
    public int TotalStudyMinutes { get; set; }

    [FirestoreProperty]
    public List<string> PreferredSkills { get; set; } = new();
}
