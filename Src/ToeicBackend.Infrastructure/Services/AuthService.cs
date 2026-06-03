using FirebaseAdmin.Auth;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ToeicBackend.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly IResetTokenService _resetTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly TimeSpan _otpExpiration = TimeSpan.FromMinutes(10);

    public AuthService(
        IUserRepository userRepository,
        IOtpService otpService,
        IResetTokenService resetTokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _resetTokenService = resetTokenService;
        _logger = logger;
    }

    public async Task<bool> SyncUserAsync(string token)
    {
        try
        {
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
            string uid = decodedToken.Uid;

            var existingUser = await _userRepository.GetByIdAsync(uid);

            decodedToken.Claims.TryGetValue("name", out object? nameObj);
            decodedToken.Claims.TryGetValue("email", out object? emailObj);
            decodedToken.Claims.TryGetValue("picture", out object? pictureObj);

            if (existingUser != null)
            {
                // Cập nhật DisplayName / AvatarUrl nếu đang rỗng (fix lần đầu sync thiếu claim)
                bool needsUpdate = false;
                if (string.IsNullOrEmpty(existingUser.DisplayName) || existingUser.DisplayName == "TOEIC User")
                {
                    var newName = nameObj?.ToString();
                    if (!string.IsNullOrEmpty(newName))
                    {
                        existingUser.DisplayName = newName;
                        needsUpdate = true;
                    }
                }
                if (string.IsNullOrEmpty(existingUser.AvatarUrl))
                {
                    var newAvatar = pictureObj?.ToString();
                    if (!string.IsNullOrEmpty(newAvatar))
                    {
                        existingUser.AvatarUrl = newAvatar;
                        needsUpdate = true;
                    }
                }
                if (needsUpdate) await _userRepository.UpdateAsync(existingUser);
                return true;
            }

            var newUser = new User
            {
                Uid = uid,
                DisplayName = nameObj?.ToString() ?? "TOEIC User",
                Email = emailObj?.ToString() ?? "",
                AvatarUrl = pictureObj?.ToString(),
                CreatedAt = DateTime.UtcNow,
                TargetScore = 0,
                CurrentLevel = "beginner",
                Plan = "free",
                StreakDays = 0,
                ExperiencePoints = 0,
                TotalStudyMinutes = 0,
                PreferredSkills = new List<string>()
            };

            await _userRepository.CreateAsync(newUser);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AuthService] SyncUser error");
            return false;
        }
    }

    // ========== PASSWORD RESET OTP METHODS ==========

    public async Task<bool> UserExistsByEmailAsync(string email)
    {
        try
        {
            var userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
            return userRecord != null;
        }
        catch (FirebaseAuthException ex)
        {
            if (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
            {
                return false;
            }
            _logger.LogError(ex, $"Error checking user existence for email: {email}");
            throw;
        }
    }

    public async Task SaveResetOtpAsync(string email, string otp)
    {
        try
        {
            await _otpService.SaveOtpAsync(email, otp, _otpExpiration);
            _logger.LogInformation($"[AuthService] OTP saved for email: {email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error saving reset OTP for email: {email}");
            throw;
        }
    }

    public async Task<bool> VerifyResetOtpAsync(string email, string otp)
    {
        try
        {
            var isValid = await _otpService.VerifyOtpAsync(email, otp);
            if (isValid)
            {
                _logger.LogInformation($"[AuthService] OTP verified for email: {email}");
            }
            else
            {
                _logger.LogWarning($"[AuthService] Invalid OTP attempt for email: {email}");
            }
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error verifying reset OTP for email: {email}");
            throw;
        }
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        try
        {
            var token = await _resetTokenService.GenerateTokenAsync(email);
            _logger.LogInformation($"[AuthService] Reset token generated for email: {email}");
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating reset token for email: {email}");
            throw;
        }
    }

    public async Task<bool> ValidatePasswordResetTokenAsync(string email, string token)
    {
        try
        {
            var isValid = await _resetTokenService.ValidateTokenAsync(email, token);
            if (!isValid)
            {
                _logger.LogWarning($"[AuthService] Invalid reset token for email: {email}");
            }
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error validating reset token for email: {email}");
            throw;
        }
    }

    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        try
        {
            var userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
            
            if (userRecord == null)
            {
                _logger.LogWarning($"[AuthService] User not found for email: {email}");
                return false;
            }

            // Update password using Firebase Admin SDK
            await FirebaseAuth.DefaultInstance.UpdateUserAsync(new UserRecordArgs()
            {
                Uid = userRecord.Uid,
                Password = newPassword,
            });

            // Invalidate token after successful reset
            await _resetTokenService.InvalidateTokenAsync(email);

            _logger.LogInformation($"[AuthService] Password reset successfully for email: {email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resetting password for email: {email}");
            throw;
        }
    }
}
