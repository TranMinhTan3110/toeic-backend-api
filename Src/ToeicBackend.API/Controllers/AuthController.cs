using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncUser([FromBody] SyncUserRequestDto request)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest(new { message = "Token không được để trống" });
        }

        var result = await _authService.SyncUserAsync(request.Token);

        if (result)
        {
            return Ok(new { message = "Đồng bộ người dùng thành công" });
        }

        return BadRequest(new { message = "Đồng bộ thất bại. Token có thể không hợp lệ hoặc đã hết hạn." });
    }
}
