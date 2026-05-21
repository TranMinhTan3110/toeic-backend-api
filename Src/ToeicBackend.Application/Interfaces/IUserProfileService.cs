using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetProfileAsync(string userId);
    Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateUserProfileDto dto);
}
