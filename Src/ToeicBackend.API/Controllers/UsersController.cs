using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToeicBackend.API.Extensions;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserProfileService _profileService;
    private readonly IAuthService _authService;

    public UsersController(IUserProfileService profileService, IAuthService authService)
    {
        _profileService = profileService;
        _authService = authService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        try
        {
            var profile = await _profileService.GetProfileAsync(userId);
            if (profile == null)
            {
                return NotFound(new { message = "User chưa được đồng bộ. Gọi POST /api/auth/sync trước." });
            }

            profile.Email = User.FindFirstValue("email") ?? profile.Email;
            profile.DisplayName = string.IsNullOrWhiteSpace(profile.DisplayName)
                ? User.FindFirstValue("name") ?? string.Empty
                : profile.DisplayName;
            profile.AvatarUrl ??= User.FindFirstValue("picture");

            return Ok(profile);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Không thể tải hồ sơ người dùng.", detail = ex.Message });
        }
    }

    [HttpPatch("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserProfileDto dto)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        try
        {
            var profile = await _profileService.UpdateProfileAsync(userId, dto);
            if (profile == null)
            {
                return NotFound(new { message = "User chưa được đồng bộ. Gọi POST /api/auth/sync trước." });
            }

            return Ok(profile);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Không thể cập nhật hồ sơ người dùng.", detail = ex.Message });
        }
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMe()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        try
        {
            await _profileService.DeleteProfileAsync(userId);
            return Ok(new { success = true, message = "Đã xóa hồ sơ người dùng." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Không thể xóa hồ sơ người dùng.", detail = ex.Message });
        }
    }

    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllAdmin(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] string? role = null)
    {
        var result = await _profileService.GetPagedUsersAdminAsync(page, pageSize, searchTerm, status, role);
        return Ok(result);
    }

    [HttpPost("admin/lock/{userId}")]
    public async Task<IActionResult> LockUser(string userId)
    {
        var result = await _profileService.LockUserAsync(userId);
        if (!result)
        {
            return NotFound(new { message = "Không tìm thấy người dùng." });
        }
        return Ok(new { success = true, message = "Đã khóa người dùng thành công." });
    }

    [HttpPost("admin/unlock/{userId}")]
    public async Task<IActionResult> UnlockUser(string userId)
    {
        var result = await _profileService.UnlockUserAsync(userId);
        if (!result)
        {
            return NotFound(new { message = "Không tìm thấy người dùng." });
        }
        return Ok(new { success = true, message = "Đã mở khóa người dùng thành công." });
    }

    [HttpPost("admin/create")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Email và Password không được để trống." });
        }
        try
        {
            // 1. Create User in Firebase Auth
            var firebaseUid = await _authService.CreateFirebaseUserAsync(dto.Email, dto.Password, dto.DisplayName ?? "");
            // 2. Create User in Firestore with role "admin"
            var createdProfile = await _profileService.CreateUserAdminAsync(firebaseUid, dto.Email, dto.DisplayName ?? "", dto.Role ?? "admin");
            return Ok(createdProfile);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Không thể tạo tài khoản admin.", detail = ex.Message });
        }
    }

    [HttpPatch("admin/edit/{userId}")]
    public async Task<IActionResult> EditAdmin(string userId, [FromBody] EditAdminDto dto)
    {
        try
        {
            // 1. Update User in Firebase Auth
            await _authService.UpdateFirebaseUserAsync(userId, dto.Email, dto.Password, dto.DisplayName);
            // 2. Update User in Firestore
            var updatedProfile = await _profileService.UpdateUserAdminAsync(userId, dto.Email ?? "", dto.DisplayName ?? "", dto.Role ?? "admin");
            if (updatedProfile == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản để cập nhật." });
            }
            return Ok(updatedProfile);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Không thể cập nhật tài khoản.", detail = ex.Message });
        }
    }

    [HttpDelete("admin/delete/{userId}")]
    public async Task<IActionResult> DeleteAdmin(string userId)
    {
        try
        {
            // 1. Delete in Firebase Auth
            await _authService.DeleteFirebaseUserAsync(userId);
            // 2. Delete in Firestore
            await _profileService.DeleteProfileAsync(userId);
            return Ok(new { success = true, message = "Đã xóa tài khoản thành công." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Không thể xóa tài khoản.", detail = ex.Message });
        }
    }

    [HttpPost("admin/assign-role/{userId}")]
    public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Role))
        {
            return BadRequest(new { message = "Vai trò không được để trống." });
        }
        try
        {
            var success = await _profileService.AssignRoleAsync(userId, dto.Role);
            if (!success)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }
            return Ok(new { success = true, message = $"Đã thay đổi vai trò thành {dto.Role} thành công." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Không thể thay đổi vai trò.", detail = ex.Message });
        }
    }
}

public class CreateAdminDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Role { get; set; }
}

public class EditAdminDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? DisplayName { get; set; }
    public string? Role { get; set; }
}

public class AssignRoleDto
{
    public string Role { get; set; } = string.Empty;
}


