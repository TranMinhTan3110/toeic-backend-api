using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.Infrastructure.Services;

public class ResetTokenService : IResetTokenService
{
    private readonly IMemoryCache _cache;
    private const string RESET_TOKEN_KEY_PREFIX = "password_reset_token_";
    private readonly TimeSpan _tokenExpiration = TimeSpan.FromMinutes(30); // Token valid for 30 minutes

    public ResetTokenService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<string> GenerateTokenAsync(string email)
    {
        var tokenBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        
        var token = Convert.ToBase64String(tokenBytes);
        var cacheKey = $"{RESET_TOKEN_KEY_PREFIX}{email.ToLower()}";
        
        _cache.Set(cacheKey, token, _tokenExpiration);
        
        return await Task.FromResult(token);
    }

    public async Task<bool> ValidateTokenAsync(string email, string token)
    {
        var cacheKey = $"{RESET_TOKEN_KEY_PREFIX}{email.ToLower()}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedToken))
        {
            if (cachedToken == token)
            {
                return true;
            }
        }
        
        return await Task.FromResult(false);
    }

    public async Task InvalidateTokenAsync(string email)
    {
        var cacheKey = $"{RESET_TOKEN_KEY_PREFIX}{email.ToLower()}";
        _cache.Remove(cacheKey);
        await Task.CompletedTask;
    }
}
