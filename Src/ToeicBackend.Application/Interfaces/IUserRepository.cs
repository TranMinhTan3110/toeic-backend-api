using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string uid);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
}
