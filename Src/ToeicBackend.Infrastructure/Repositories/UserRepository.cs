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

    public async Task<User?> GetByIdAsync(string uid)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(uid);
        var snapshot = await docRef.GetSnapshotAsync();
        
        if (!snapshot.Exists) return null;
        
        return MapToDomain(snapshot);
    }

    public async Task CreateAsync(User user)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(user.Uid);
        await docRef.SetAsync(MapFromDomain(user));
    }

    public async Task UpdateAsync(User user)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(user.Uid);
        await docRef.SetAsync(MapFromDomain(user), SetOptions.MergeAll);
    }

    private User MapToDomain(DocumentSnapshot doc)
    {
        return new User
        {
            Uid = doc.Id,
            DisplayName = doc.GetValue<string>("display_name"),
            Email = doc.GetValue<string>("email"),
            AvatarUrl = doc.ContainsField("avatar_url") ? doc.GetValue<string?>("avatar_url") : null,
            TargetScore = doc.ContainsField("target_score") ? doc.GetValue<int>("target_score") : 0,
            CurrentLevel = doc.ContainsField("current_level") ? doc.GetValue<string>("current_level") : "beginner",
            Plan = doc.ContainsField("plan") ? doc.GetValue<string>("plan") : "free",
            StreakDays = doc.ContainsField("streak_days") ? doc.GetValue<int>("streak_days") : 0,
            ExperiencePoints = doc.ContainsField("experience_points") ? doc.GetValue<int>("experience_points") : 0,
            LastStudyDate = doc.ContainsField("last_study_date") ? doc.GetValue<DateTime?>("last_study_date") : null,
            TotalStudyMinutes = doc.ContainsField("total_study_minutes") ? doc.GetValue<int>("total_study_minutes") : 0,
            PreferredSkills = doc.ContainsField("preferred_skills") ? doc.GetValue<List<string>>("preferred_skills") : new(),
            CreatedAt = doc.ContainsField("created_at") ? doc.GetValue<DateTime>("created_at") : DateTime.UtcNow
        };
    }

    private Dictionary<string, object> MapFromDomain(User user)
    {
        var data = new Dictionary<string, object>
        {
            { "uid", user.Uid },
            { "display_name", user.DisplayName },
            { "email", user.Email },
            { "target_score", user.TargetScore },
            { "current_level", user.CurrentLevel },
            { "plan", user.Plan },
            { "streak_days", user.StreakDays },
            { "experience_points", user.ExperiencePoints },
            { "total_study_minutes", user.TotalStudyMinutes },
            { "preferred_skills", user.PreferredSkills },
            { "created_at", user.CreatedAt }
        };

        if (user.AvatarUrl != null) data["avatar_url"] = user.AvatarUrl;
        if (user.LastStudyDate != null) data["last_study_date"] = Timestamp.FromDateTime(user.LastStudyDate.Value.ToUniversalTime());

        return data;
    }
}
