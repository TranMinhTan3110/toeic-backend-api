using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToeicBackend.API.Extensions;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/exam")]
public class ExamController : ControllerBase
{
    private readonly IExamService _service;

    public ExamController(IExamService service)
    {
        _service = service;
    }

    [HttpPost("history/submit")]
    [Authorize]
    public async Task<IActionResult> SubmitFullTest([FromBody] SaveFullTestRequestDto dto)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        try
        {
            var id = await _service.SaveFullTestHistoryAsync(userId, dto);
            return Ok(new { success = true, id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi nộp bài thi Full Test", detail = ex.Message });
        }
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<FullTestHistoryDto>>> GetUserHistory()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var results = await _service.GetUserFullTestHistoryAsync(userId);
        return Ok(results);
    }

    [HttpGet("history/{id}")]
    [Authorize]
    public async Task<ActionResult<FullTestHistoryDto>> GetHistoryById(string id)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var result = await _service.GetFullTestHistoryByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { message = "Không tìm thấy lịch sử thi Full Test" });
        }

        if (result.UserId != userId)
        {
            return Forbid();
        }

        return Ok(result);
    }


    [HttpGet("questions/{examId}")]
    public async Task<ActionResult<IEnumerable<ListeningQuestionDto>>> GetExamQuestions(string examId)
    {
        var results = await _service.GetExamQuestionsAsync(examId);
        return Ok(results);
    }

    [HttpGet("groups/{examId}")]
    public async Task<ActionResult<IEnumerable<ListeningGroupDto>>> GetExamGroups(string examId)
    {
        var results = await _service.GetExamGroupsAsync(examId);
        return Ok(results);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExamDto>>> GetAll()
    {
        var results = await _service.GetAllAsync();
        return Ok(results);
    }

    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<ExamDto>>> GetByFilter([FromQuery] bool? isExam, [FromQuery] bool? isPractice)
    {
        var results = await _service.GetByFilterAsync(isExam, isPractice);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExamDto>> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(new { Message = "Exam not found" });
        return Ok(result);
    }
}
