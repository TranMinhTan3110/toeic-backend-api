using Google.Cloud.Firestore;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string CollectionName = "users";

    public UserRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    // Đọc thẳng từ dictionary để tránh lỗi encoding với tên field tiếng Việt
    private static User SnapshotToUser(DocumentSnapshot snap)
    {
        var d = snap.ToDictionary();
        T Get<T>(string key, T fallback = default!) =>
            d.TryGetValue(key, out var v) && v is T t ? t : fallback;

        DateTime createdAt = DateTime.UtcNow;
        if (d.TryGetValue("created_at", out var ca) && ca is Timestamp ts)
            createdAt = ts.ToDateTime(); // ToDateTime() luôn trả về UTC

        return new User
        {
            Uid              = snap.Id,
            DisplayName      = Get<string>("display_name", ""),
            Email            = Get<string>("email", ""),
            AvatarUrl        = Get<string>("avatar_url", null!),
            CreatedAt        = createdAt,
            TargetScore      = (int)Get<long>("target_score", 0),
            CurrentLevel     = Get<string>("current_level", "beginner"),
            Plan             = Get<string>("plan", "free"),
            StreakDays       = (int)Get<long>("streak_days", 0),
            BestStreakDays   = (int)Get<long>("best_streak_days", 0),
            LastStudyDate    = Get<string>("last_study_date", null!),
            ExperiencePoints = (int)Get<long>("experience_points", 0),
            WeeklyEp         = (int)Get<long>("weekly_ep", 0),
            WeeklyEpPeriodKey= Get<string>("weekly_ep_period_key", ""),
            DailyEpDateKey   = Get<string>("daily_ep_date_key", null!),
            DailyEpEarned    = (int)Get<long>("daily_ep_earned", 0),
            TotalStudyMinutes= (int)Get<long>("total_study_minutes", 0),
            PreferredSkills  = d.TryGetValue("preferred_skills", out var ps) && ps is List<object> list
                                ? list.Select(x => x.ToString()!).ToList()
                                : new List<string>(),
            IsLocked         = Get<bool>("is_locked", false),
            Role             = Get<string>("role", Get<string>("email", "").Contains("admin") || Get<string>("email", "") == "nguyengamo@gmail.com" ? "admin" : "user"),
            PhoneNumber      = Get<string>("phone_number", null!),
            Gender           = Get<string>("gender", null!),
            BirthDate        = Get<string>("birth_date", null!),
        };
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return null;
        return SnapshotToUser(snapshot);
    }

    public async Task CreateAsync(User user)
    {
        user.CreatedAt = DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc);
        var docRef = _firestoreDb.Collection(CollectionName).Document(user.Uid);
        await docRef.SetAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        user.CreatedAt = DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc);
        var docRef = _firestoreDb.Collection(CollectionName).Document(user.Uid);
        await docRef.SetAsync(user, SetOptions.MergeAll);
    }

    public async Task<IReadOnlyList<User>> GetWeeklyLeaderboardAsync(string periodKey, int limit)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("weekly_ep_period_key", periodKey)
            .OrderByDescending("weekly_ep")
            .Limit(limit);

        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents
            .Select(SnapshotToUser)
            .Where(u => u.WeeklyEp > 0)
            .ToList();
    }

    public async Task<IReadOnlyList<User>> GetAllUsersAsync()
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).GetSnapshotAsync();
        return snapshot.Documents
            .Select(SnapshotToUser)
            .ToList();
    }
}
