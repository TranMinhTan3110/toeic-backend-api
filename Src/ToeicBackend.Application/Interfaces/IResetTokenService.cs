namespace ToeicBackend.Application.Interfaces;

public interface IResetTokenService
{
    Task<string> GenerateTokenAsync(string email);
    Task<bool> ValidateTokenAsync(string email, string token);
    Task InvalidateTokenAsync(string email);
}
