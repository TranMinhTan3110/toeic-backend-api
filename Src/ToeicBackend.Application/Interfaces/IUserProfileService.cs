using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetProfileAsync(string userId);
    Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateUserProfileDto dto);
    Task DeleteProfileAsync(string userId);
    Task<IEnumerable<UserProfileDto>> GetAllUsersAdminAsync();
    Task<PagedUsersResultDto> GetPagedUsersAdminAsync(int page, int pageSize, string? searchTerm, string? status, string? role);
    Task<bool> LockUserAsync(string userId);
    Task<bool> UnlockUserAsync(string userId);
}


