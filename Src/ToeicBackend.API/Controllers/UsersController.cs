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
}
