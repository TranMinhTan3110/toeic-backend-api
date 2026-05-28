using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToeicBackend.API.Extensions;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/writing")]
[Route("api/writing-history")]
[Route("api/writing-submissions")]
public class WritingHistoryController : ControllerBase
{
    private readonly IWritingHistoryService _service;

    public WritingHistoryController(IWritingHistoryService service)
    {
        _service = service;
    }

    [HttpPost]
    [HttpPost("history")]
    [HttpPost("submit")]
    [Authorize]
    public async Task<IActionResult> SaveHistory([FromBody] SaveWritingHistoryRequestDto dto)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        if (string.IsNullOrWhiteSpace(dto.QuestionId) || string.IsNullOrWhiteSpace(dto.UserAnswer))
        {
            return BadRequest(new { message = "QuestionId và UserAnswer không được để trống" });
        }

        try
        {
            var historyId = await _service.SaveHistoryAsync(userId, dto);
            return Ok(new { success = true, id = historyId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("evaluate")]
    [Authorize]
    public async Task<ActionResult<WritingEvaluationDto>> Evaluate([FromBody] EvaluateWritingRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.QuestionId))
        {
            return BadRequest(new { message = "questionId là bắt buộc" });
        }

        try
        {
            var result = await _service.EvaluateAsync(dto.QuestionId, dto.UserAnswer);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi chấm điểm AI", detail = ex.Message });
        }
    }

    [HttpPost("session")]
    [Authorize]
    public async Task<IActionResult> SaveSession([FromBody] SaveWritingSessionRequestDto dto)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        if (dto.QuestionIds == null || dto.QuestionIds.Count == 0)
        {
            return BadRequest(new { message = "QuestionIds không được để trống" });
        }

        try
        {
            var historyId = await _service.SaveSessionAsync(userId, dto);
            return Ok(new { success = true, id = historyId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet]
    [HttpGet("history")]
    [HttpGet("submissions")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<WritingHistoryDto>>> GetUserHistory([FromQuery] string? sessionType)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var results = await _service.GetUserHistoryAsync(userId, sessionType);
        return Ok(results);
    }

    [HttpGet("{id}")]
    [HttpGet("history/{id}")]
    [Authorize]
    public async Task<ActionResult<WritingHistoryDto>> GetHistoryById(string id)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var result = await _service.GetHistoryByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { message = "Không tìm thấy lịch sử writing" });
        }

        if (result.UserId != userId)
        {
            return Forbid();
        }

        return Ok(result);
    }
}
