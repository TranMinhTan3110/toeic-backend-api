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
            // 1. Verify Firebase ID Token
            // Token này được gửi từ Frontend sau khi Login thành công
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
            string uid = decodedToken.Uid;

            // 2. Kiểm tra xem User đã tồn tại trong Firestore chưa
            var existingUser = await _userRepository.GetByIdAsync(uid);
            if (existingUser != null)
            {
                // Nếu đã tồn tại thì thôi, trả về thành công
                return true;
            }

            // 3. Nếu chưa có, tạo mới hồ sơ User
            // Lấy thêm thông tin cơ bản từ các Claims trong Token (Email, Name, Picture)
            decodedToken.Claims.TryGetValue("name", out object? nameObj);
            decodedToken.Claims.TryGetValue("email", out object? emailObj);
            decodedToken.Claims.TryGetValue("picture", out object? pictureObj);

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
            // Log lỗi (ví dụ: Token hết hạn hoặc không hợp lệ)
            Console.WriteLine($"[AuthService] SyncUser error: {ex.Message}");
            return false;
        }
    }
}
