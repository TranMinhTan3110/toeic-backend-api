using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToeicBackend.API.Extensions;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/speaking")]
public class SpeakingController : ControllerBase
{
    private readonly ISpeakingService _service;
    private readonly ISpeakingHistoryService _historyService;

    public SpeakingController(ISpeakingService service, ISpeakingHistoryService historyService)
    {
        _service = service;
        _historyService = historyService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpeakingQuestionDto>>> GetAll()
    {
        var results = await _service.GetAllAsync();
        return Ok(results);
    }

    [HttpGet("task/{taskNumber}")]
    public async Task<ActionResult<IEnumerable<SpeakingQuestionDto>>> GetByTaskNumber(
        int taskNumber,
        [FromQuery] bool? isExam,
        [FromQuery] bool? isPractice)
    {
        var results = await _service.GetByTaskNumberAsync(taskNumber, isExam, isPractice);
        return Ok(results);
    }

    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<SpeakingQuestionDto>>> GetByFilter([FromQuery] bool? isExam, [FromQuery] bool? isPractice)
    {
        var results = await _service.GetByFilterAsync(isExam, isPractice);
        return Ok(results);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount([FromQuery] bool? isExam, [FromQuery] bool? isPractice)
    {
        var result = await _service.GetCountByFilterAsync(isExam, isPractice);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SpeakingQuestionDto>> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(new { Message = "Speaking question not found" });
        return Ok(result);
    }

    [HttpGet("exam/{examSetId}")]
    public async Task<ActionResult<IEnumerable<SpeakingQuestionDto>>> GetByExamSetId(string examSetId)
    {
        var results = await _service.GetByExamSetIdAsync(examSetId);
        return Ok(results);
    }

    /// <summary>Chấm bài nói bằng Gemini — so sánh transcript với bài mẫu trên Firestore.</summary>
    [HttpPost("evaluate")]
    [Authorize]
    public async Task<ActionResult<SpeakingEvaluationDto>> Evaluate(
        [FromForm] string questionId,
        [FromForm] string? transcript,
        [FromForm] int? subQuestionIndex,
        IFormFile? audio = null)
    {
        if (string.IsNullOrWhiteSpace(questionId))
        {
            return BadRequest(new { message = "questionId là bắt buộc" });
        }

        try
        {
            byte[]? audioBytes = null;
            string? mimeType = null;

            if (audio != null && audio.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await audio.CopyToAsync(ms);
                    audioBytes = ms.ToArray();
                }
                mimeType = audio.ContentType;
            }

            var result = await _historyService.EvaluateAsync(
                questionId,
                transcript ?? string.Empty,
                subQuestionIndex,
                audioBytes,
                mimeType);
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

    [HttpPost("history")]
    [Authorize]
    public async Task<IActionResult> SaveHistory([FromBody] SaveSpeakingHistoryRequestDto dto)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var historyId = await _historyService.SaveHistoryAsync(userId, dto);
        return Ok(new { success = true, id = historyId });
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<SpeakingHistoryDto>>> GetUserHistory(
        [FromQuery] string? sessionType)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var results = await _historyService.GetUserHistoryAsync(userId, sessionType);
        return Ok(results);
    }

    [HttpGet("history/{id}")]
    [Authorize]
    public async Task<ActionResult<SpeakingHistoryDto>> GetHistoryById(string id)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var result = await _historyService.GetHistoryByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { message = "Không tìm thấy lịch sử với ID này" });
        }

        if (result.UserId != userId)
        {
            return Forbid();
        }

        return Ok(result);
    }
}
