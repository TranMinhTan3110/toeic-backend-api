using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Repositories;

public class SpeakingHistoryRepository : ISpeakingHistoryRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly IMemoryCache _cache;
    private const string CollectionName = "user_speaking_history";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public SpeakingHistoryRepository(FirestoreDb firestoreDb, IMemoryCache cache)
    {
        _firestoreDb = firestoreDb;
        _cache = cache;
    }

    public async Task<string> AddAsync(SpeakingHistory history)
    {
        if (string.IsNullOrEmpty(history.Id))
        {
            var docRef = await _firestoreDb.Collection(CollectionName).AddAsync(history);
            history.Id = docRef.Id;
        }
        else
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(history.Id);
            await docRef.SetAsync(history, SetOptions.Overwrite);
        }

        _cache.Remove(UserListCacheKey(history.UserId));
        return history.Id;
    }

    public async Task<IEnumerable<SpeakingHistory>> GetByUserIdAsync(string userId, string? sessionType = null)
    {
        var cacheKey = UserListCacheKey(userId, sessionType);
        if (_cache.TryGetValue(cacheKey, out List<SpeakingHistory>? cached) && cached != null)
        {
            return cached;
        }

        Query query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("user_id", userId);

        if (!string.IsNullOrEmpty(sessionType))
        {
            query = query.WhereEqualTo("session_type", sessionType);
        }

        var snapshot = await query.GetSnapshotAsync();
        var results = snapshot.Documents
            .Select(doc => doc.ConvertTo<SpeakingHistory>())
            .OrderByDescending(h => h.Date)
            .Take(30)
            .ToList();

        _cache.Set(cacheKey, results, CacheDuration);
        return results;
    }

    public async Task<SpeakingHistory?> GetByIdAsync(string id)
    {
        var cacheKey = $"speaking_history_detail_{id}";
        if (_cache.TryGetValue(cacheKey, out SpeakingHistory? cached) && cached != null)
        {
            return cached;
        }

        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return null;

        var result = snapshot.ConvertTo<SpeakingHistory>();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        return result;
    }

    private static string UserListCacheKey(string userId, string? sessionType = null) =>
        $"user_speaking_history_list_{userId}_{sessionType ?? "all"}";
}
