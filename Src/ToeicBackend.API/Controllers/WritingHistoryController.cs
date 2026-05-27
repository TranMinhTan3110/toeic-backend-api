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

        var hasSingleAnswer = !string.IsNullOrWhiteSpace(dto.QuestionId) && !string.IsNullOrWhiteSpace(dto.UserAnswer);
        var hasMultipleAnswers = dto.QuestionIds != null && dto.QuestionIds.Any() && dto.Answers != null && dto.Answers.Any();

        if (!hasSingleAnswer && !hasMultipleAnswers)
        {
            return BadRequest(new { message = "Thông tin lịch sử writing không hợp lệ. Cần questionId+userAnswer hoặc questionIds+answers." });
        }

        var historyId = await _service.SaveHistoryAsync(userId, dto);
        return Ok(new { success = true, id = historyId });
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
