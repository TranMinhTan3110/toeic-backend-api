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

    public UsersController(IUserProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var profile = await _profileService.GetProfileAsync(userId);
        if (profile == null)
        {
            return NotFound(new { message = "User chưa được đồng bộ. Gọi POST /api/auth/sync trước." });
        }

        return Ok(profile);
    }

    [HttpPatch("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserProfileDto dto)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var profile = await _profileService.UpdateProfileAsync(userId, dto);
        if (profile == null)
        {
            return NotFound(new { message = "User chưa được đồng bộ. Gọi POST /api/auth/sync trước." });
        }

        return Ok(profile);
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
}

