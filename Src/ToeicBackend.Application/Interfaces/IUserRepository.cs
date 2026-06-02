using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task UpdateFieldsAsync(string id, IReadOnlyDictionary<string, object?> fields);
    Task<IReadOnlyList<User>> GetWeeklyLeaderboardAsync(string periodKey, int limit);
    Task<IReadOnlyList<User>> GetAllUsersAsync();
}

