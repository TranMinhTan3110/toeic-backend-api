using Google.Cloud.Firestore;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Repositories;

public class VocabularyProgressRepository : IVocabularyProgressRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string CollectionName = "user_vocabulary_progress";

    public VocabularyProgressRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<UserVocabularyProgress?> GetAsync(string userId, string vocabularyId)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("UserId", userId)
            .WhereEqualTo("VocabularyId", vocabularyId);

        var snapshot = await query.GetSnapshotAsync();
        var doc = snapshot.Documents.FirstOrDefault();

        if (doc == null) return null;

        return doc.ConvertTo<UserVocabularyProgress>();
    }

    public async Task UpsertAsync(UserVocabularyProgress progress)
    {
        if (string.IsNullOrEmpty(progress.Id))
        {
            // Thêm mới
            await _firestoreDb.Collection(CollectionName).AddAsync(progress);
        }
        else
        {
            // Cập nhật
            var docRef = _firestoreDb.Collection(CollectionName).Document(progress.Id);
            await docRef.SetAsync(progress, SetOptions.Overwrite);
        }
    }

    public async Task<IEnumerable<UserVocabularyProgress>> GetUserProgressAsync(string userId)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("UserId", userId);

        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<UserVocabularyProgress>());
    }

    public async Task<IEnumerable<string>> GetDueVocabularyIdsAsync(string userId)
    {
        var now = DateTime.UtcNow;
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("UserId", userId)
            .WhereLessThanOrEqualTo("NextReviewDate", now);

        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.GetValue<string>("VocabularyId"));
    }
}
