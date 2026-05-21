using FirebaseAdmin.Auth;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
            Console.WriteLine($"[AuthService] SyncUser error: {ex.Message}");
            return false;
        }
    }
}
