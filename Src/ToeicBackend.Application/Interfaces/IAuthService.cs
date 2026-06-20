namespace ToeicBackend.Application.Interfaces;

public interface IAuthService
{
    Task<bool> SyncUserAsync(string token);
    
    // Password Reset OTP methods
    Task<bool> UserExistsByEmailAsync(string email);
    Task SaveResetOtpAsync(string email, string otp);
    Task<bool> VerifyResetOtpAsync(string email, string otp);
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ValidatePasswordResetTokenAsync(string email, string token);
    Task<bool> ResetPasswordAsync(string email, string newPassword);

    // Administrative user actions
    Task<string> CreateFirebaseUserAsync(string email, string password, string displayName);
    Task UpdateFirebaseUserAsync(string uid, string? email, string? password, string? displayName);
    Task DeleteFirebaseUserAsync(string uid);
}

