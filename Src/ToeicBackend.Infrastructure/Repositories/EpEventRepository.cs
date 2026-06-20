using Google.Cloud.Firestore;
using Grpc.Core;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Repositories;

public class EpEventRepository : IEpEventRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string CollectionName = "ep_events";

    public EpEventRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task AddAsync(EpEvent epEvent)
    {
        await _firestoreDb.Collection(CollectionName).AddAsync(epEvent);
    }

    public async Task<int> SumEpForUserOnDateAsync(string userId, string studyDateKey)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo(nameof(EpEvent.UserId), userId)
            .WhereEqualTo(nameof(EpEvent.StudyDateKey), studyDateKey);

        try
        {
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Sum(d => d.ConvertTo<EpEvent>().EpAwarded);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.FailedPrecondition)
        {
            Console.WriteLine("[EpEventRepository] Thiếu Firestore index (sum EP/ngày). Tạo index theo Docs/Firestore_Indexes_EP.md");
            return 0;
        }
    }

    public async Task<bool> ExistsForReferenceOnDateAsync(
        string userId,
        string activityType,
        string referenceId,
        string studyDateKey)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo(nameof(EpEvent.UserId), userId)
            .WhereEqualTo(nameof(EpEvent.ActivityType), activityType)
            .WhereEqualTo(nameof(EpEvent.ReferenceId), referenceId)
            .WhereEqualTo(nameof(EpEvent.StudyDateKey), studyDateKey)
            .Limit(1);

        try
        {
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Count > 0;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.FailedPrecondition)
        {
            Console.WriteLine("[EpEventRepository] Thiếu Firestore index (check trùng từ/ngày). Tạo index theo Docs/Firestore_Indexes_EP.md");
            return false;
        }
    }

    public async Task<bool> ExistsForReferenceEverAsync(
        string userId,
        string activityType,
        string referenceId)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo(nameof(EpEvent.UserId), userId)
            .WhereEqualTo(nameof(EpEvent.ActivityType), activityType)
            .WhereEqualTo(nameof(EpEvent.ReferenceId), referenceId)
            .Limit(1);

        try
        {
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Count > 0;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.FailedPrecondition)
        {
            Console.WriteLine("[EpEventRepository] Thiếu Firestore index (ExistsForReferenceEver).");
            return false;
        }
    }
}
