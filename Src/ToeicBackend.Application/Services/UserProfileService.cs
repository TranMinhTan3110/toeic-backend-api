using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepository;

    public UserProfileService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto?> GetProfileAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateUserProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        if (dto.TargetScore.HasValue) user.TargetScore = dto.TargetScore.Value;
        if (!string.IsNullOrWhiteSpace(dto.CurrentLevel)) user.CurrentLevel = dto.CurrentLevel;
        if (dto.PreferredSkills != null) user.PreferredSkills = dto.PreferredSkills;

        await _userRepository.UpdateAsync(user);
        return MapToDto(user);
    }

    private static UserProfileDto MapToDto(Domain.Entities.User user) => new()
    {
        Uid = user.Uid,
        DisplayName = user.DisplayName,
        Email = user.Email,
        AvatarUrl = user.AvatarUrl,
        TargetScore = user.TargetScore,
        CurrentLevel = user.CurrentLevel,
        Plan = user.Plan,
        PreferredSkills = user.PreferredSkills,
        ExperiencePoints = user.ExperiencePoints,
        WeeklyEp = user.WeeklyEp,
        WeeklyEpPeriodKey = user.WeeklyEpPeriodKey,
        StreakDays = user.StreakDays,
        BestStreakDays = user.BestStreakDays,
        TotalStudyMinutes = user.TotalStudyMinutes,
        CreatedAt = user.CreatedAt
    };
}
