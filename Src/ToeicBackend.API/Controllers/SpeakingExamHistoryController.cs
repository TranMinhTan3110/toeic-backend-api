using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToeicBackend.API.Extensions;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/speaking-exam/history")]
public class SpeakingExamHistoryController : ControllerBase
{
    private readonly ISpeakingExamHistoryService _service;

    public SpeakingExamHistoryController(ISpeakingExamHistoryService service)
    {
        _service = service;
    }

    [HttpPost("submit")]
    [Authorize]
    public async Task<IActionResult> SubmitExam([FromBody] SaveSpeakingExamRequestDto dto)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        try
        {
            var result = await _service.SubmitExamAsync(userId, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi chấm điểm thi Speaking", detail = ex.Message });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<SpeakingExamHistoryDto>>> GetUserHistory()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var results = await _service.GetUserHistoryAsync(userId);
        return Ok(results);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<SpeakingExamHistoryDto>> GetById(string id)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var result = await _service.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { message = "Không tìm thấy lịch sử thi Speaking" });
        }

        if (result.UserId != userId)
        {
            return Forbid();
        }

        return Ok(result);
    }
}
