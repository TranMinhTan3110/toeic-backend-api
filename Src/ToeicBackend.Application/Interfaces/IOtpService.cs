namespace ToeicBackend.Application.Interfaces;

public interface IOtpService
{
    Task SaveOtpAsync(string email, string otp, TimeSpan expiration);
    Task<bool> VerifyOtpAsync(string email, string otp);
    Task InvalidateOtpAsync(string email);
}
