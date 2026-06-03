using Microsoft.Extensions.Caching.Memory;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private const string OTP_KEY_PREFIX = "password_reset_otp_";

    public OtpService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task SaveOtpAsync(string email, string otp, TimeSpan expiration)
    {
        var cacheKey = $"{OTP_KEY_PREFIX}{email.ToLower()}";
        _cache.Set(cacheKey, otp, expiration);
        await Task.CompletedTask;
    }

    public async Task<bool> VerifyOtpAsync(string email, string otp)
    {
        var cacheKey = $"{OTP_KEY_PREFIX}{email.ToLower()}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedOtp))
        {
            if (cachedOtp == otp)
            {
                _cache.Remove(cacheKey);
                return true;
            }
        }
        
        return await Task.FromResult(false);
    }

    public async Task InvalidateOtpAsync(string email)
    {
        var cacheKey = $"{OTP_KEY_PREFIX}{email.ToLower()}";
        _cache.Remove(cacheKey);
        await Task.CompletedTask;
    }
}
