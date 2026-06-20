using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToeicBackend.API.Extensions;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/writing-exam/history")]
public class WritingExamHistoryController : ControllerBase
{
    private readonly IWritingExamHistoryService _service;

    public WritingExamHistoryController(IWritingExamHistoryService service)
    {
        _service = service;
    }

    [HttpPost("submit")]
    [Authorize]
    public async Task<IActionResult> SubmitExam([FromBody] SaveWritingExamRequestDto dto)
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
            return StatusCode(500, new { message = "Lỗi chấm điểm thi Writing", detail = ex.Message });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<WritingExamHistoryDto>>> GetUserHistory()
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
    public async Task<ActionResult<WritingExamHistoryDto>> GetById(string id)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var result = await _service.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { message = "Không tìm thấy lịch sử thi Writing" });
        }

        if (result.UserId != userId)
        {
            return Forbid();
        }

        return Ok(result);
    }
}
