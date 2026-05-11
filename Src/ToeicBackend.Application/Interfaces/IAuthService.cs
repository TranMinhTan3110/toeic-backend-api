namespace ToeicBackend.Application.Interfaces;

public interface IAuthService
{
    Task<bool> SyncUserAsync(string token);
}
