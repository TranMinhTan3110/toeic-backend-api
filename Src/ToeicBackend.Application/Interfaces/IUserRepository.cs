using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task CreateAsync(User user);
}
