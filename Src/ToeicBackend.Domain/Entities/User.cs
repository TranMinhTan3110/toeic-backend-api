using Google.Cloud.Firestore;

namespace ToeicBackend.Domain.Entities;

[FirestoreData]
public class User
{
    [FirestoreDocumentId]
    public string Uid { get; set; } = string.Empty;

    [FirestoreProperty("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [FirestoreProperty("email")]
    public string Email { get; set; } = string.Empty;

    [FirestoreProperty("avatar_url")]
    public string? AvatarUrl { get; set; }

    [FirestoreProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [FirestoreProperty("target_score")]
    public int TargetScore { get; set; }

    [FirestoreProperty("current_level")]
    public string CurrentLevel { get; set; } = "beginner";

    [FirestoreProperty("plan")]
    public string Plan { get; set; } = "free";

    [FirestoreProperty("streak_days")]
    public int StreakDays { get; set; }

    [FirestoreProperty("best_streak_days")]
    public int BestStreakDays { get; set; }

    /// <summary>yyyy-MM-dd theo múi giờ Asia/Ho_Chi_Minh.</summary>
    [FirestoreProperty("last_study_date")]
    public string? LastStudyDate { get; set; }

    [FirestoreProperty("experience_points")]
    public int ExperiencePoints { get; set; }

    [FirestoreProperty("weekly_ep")]
    public int WeeklyEp { get; set; }

    [FirestoreProperty("weekly_ep_period_key")]
    public string WeeklyEpPeriodKey { get; set; } = string.Empty;

    /// <summary>yyyy-MM-dd VN — ngày đã tính daily EP.</summary>
    [FirestoreProperty("daily_ep_date_key")]
    public string? DailyEpDateKey { get; set; }

    [FirestoreProperty("daily_ep_earned")]
    public int DailyEpEarned { get; set; }

    [FirestoreProperty("total_study_minutes")]
    public int TotalStudyMinutes { get; set; }

    [FirestoreProperty("preferred_skills")]
    public List<string> PreferredSkills { get; set; } = new();
}
