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

    public async Task<User?> GetByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists) return null;

        return snapshot.ConvertTo<User>();
    }

    public async Task CreateAsync(User user)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(user.Uid);
        await docRef.SetAsync(user);
    }
}
