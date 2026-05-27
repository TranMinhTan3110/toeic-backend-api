using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Repositories;

public class WritingHistoryRepository : IWritingHistoryRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly IMemoryCache _cache;
    private const string CollectionName = "user_writing_history";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public WritingHistoryRepository(FirestoreDb firestoreDb, IMemoryCache cache)
    {
        _firestoreDb = firestoreDb;
        _cache = cache;
    }

    public async Task<string> AddAsync(WritingHistory history)
    {
        if (string.IsNullOrWhiteSpace(history.Id))
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

    public async Task<IEnumerable<WritingHistory>> GetByUserIdAsync(string userId, string? sessionType = null)
    {
        var cacheKey = UserListCacheKey(userId, sessionType);
        if (_cache.TryGetValue(cacheKey, out List<WritingHistory>? cached) && cached != null)
        {
            return cached;
        }

        Query query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("user_id", userId);

        if (!string.IsNullOrWhiteSpace(sessionType))
        {
            query = query.WhereEqualTo("session_type", sessionType);
        }

        var snapshot = await query.GetSnapshotAsync();
        var results = snapshot.Documents
            .Select(doc => doc.ConvertTo<WritingHistory>())
            .OrderByDescending(history => history.SubmittedAt)
            .Take(30)
            .ToList();

        _cache.Set(cacheKey, results, CacheDuration);
        return results;
    }

    public async Task<WritingHistory?> GetByIdAsync(string id)
    {
        var cacheKey = WritingHistoryDetailCacheKey(id);
        if (_cache.TryGetValue(cacheKey, out WritingHistory? cached) && cached != null)
        {
            return cached;
        }

        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return null;

        var result = snapshot.ConvertTo<WritingHistory>();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        return result;
    }

    private static string UserListCacheKey(string userId, string? sessionType = null) =>
        $"user_writing_history_list_{userId}_{sessionType ?? "all"}";

    private static string WritingHistoryDetailCacheKey(string id) =>
        $"writing_history_detail_{id}";
}
