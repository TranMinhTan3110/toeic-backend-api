using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;
    private const string AdminUsersCacheKey = "admin_all_users";
    private static readonly TimeSpan UsersCacheDuration = TimeSpan.FromMinutes(30);

    public UserProfileService(IUserRepository userRepository, IMemoryCache cache)
    {
        _userRepository = userRepository;
        _cache = cache;
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
        
        // Cập nhật các trường thông tin cá nhân mới
        if (dto.DisplayName != null) user.DisplayName = dto.DisplayName;
        if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;
        if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
        if (dto.Gender != null) user.Gender = dto.Gender;
        if (dto.BirthDate != null) user.BirthDate = dto.BirthDate;

        await _userRepository.UpdateAsync(user);
        
        // Xóa cache vì thông tin user đã thay đổi
        _cache.Remove(AdminUsersCacheKey);

        return MapToDto(user);
    }

    public async Task<IEnumerable<UserProfileDto>> GetAllUsersAdminAsync()
    {
        if (_cache.TryGetValue(AdminUsersCacheKey, out List<UserProfileDto>? cachedUsers) && cachedUsers != null)
        {
            return cachedUsers;
        }

        var users = await _userRepository.GetAllUsersAsync();
        var dtos = users.Select(MapToDto).ToList();

        _cache.Set(AdminUsersCacheKey, dtos, UsersCacheDuration);
        return dtos;
    }

    public async Task<PagedUsersResultDto> GetPagedUsersAdminAsync(
        int page, 
        int pageSize, 
        string? searchTerm, 
        string? status, 
        string? role)
    {
        // 1. Lấy toàn bộ danh sách users từ cache hoặc DB (quota-safe!)
        var allUsers = await GetAllUsersAdminAsync();

        // 2. Lọc theo từ khóa tìm kiếm (Tên hoặc Email)
        var filteredQuery = allUsers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            filteredQuery = filteredQuery.Where(u => 
                (u.DisplayName != null && u.DisplayName.ToLower().Contains(searchLower)) || 
                (u.Email != null && u.Email.ToLower().Contains(searchLower)));
        }

        // 3. Lọc theo trạng thái (Active, Locked)
        if (!string.IsNullOrWhiteSpace(status) && status != "all")
        {
            if (status == "locked")
                filteredQuery = filteredQuery.Where(u => u.IsLocked);
            else if (status == "active")
                filteredQuery = filteredQuery.Where(u => !u.IsLocked);
        }

        // 4. Lọc theo vai trò (User, Admin)
        if (!string.IsNullOrWhiteSpace(role) && role != "all")
        {
            var roleLower = role.ToLower();
            filteredQuery = filteredQuery.Where(u => u.Role.ToLower() == roleLower);
        }

        // 5. Phân trang
        var totalCount = filteredQuery.Count();
        var items = filteredQuery
            .OrderByDescending(u => u.CreatedAt) // Mới nhất lên đầu
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedUsersResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> LockUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.IsLocked = true;
        await _userRepository.UpdateAsync(user);

        // Cập nhật cache in-memory trực tiếp để tiết kiệm số lần đọc Firebase
        if (_cache.TryGetValue(AdminUsersCacheKey, out List<UserProfileDto>? cachedUsers) && cachedUsers != null)
        {
            var cachedUser = cachedUsers.FirstOrDefault(u => u.Uid == userId);
            if (cachedUser != null)
            {
                cachedUser.IsLocked = true;
            }
        }
        else
        {
            _cache.Remove(AdminUsersCacheKey);
        }

        return true;
    }

    public async Task<bool> UnlockUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.IsLocked = false;
        await _userRepository.UpdateAsync(user);

        // Cập nhật cache in-memory trực tiếp để tiết kiệm số lần đọc Firebase
        if (_cache.TryGetValue(AdminUsersCacheKey, out List<UserProfileDto>? cachedUsers) && cachedUsers != null)
        {
            var cachedUser = cachedUsers.FirstOrDefault(u => u.Uid == userId);
            if (cachedUser != null)
            {
                cachedUser.IsLocked = false;
            }
        }
        else
        {
            _cache.Remove(AdminUsersCacheKey);
        }

        return true;
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
        CreatedAt = user.CreatedAt,
        IsLocked = user.IsLocked,
        Role = user.Role,
        PhoneNumber = user.PhoneNumber,
        Gender = user.Gender,
        BirthDate = user.BirthDate
    };
}
