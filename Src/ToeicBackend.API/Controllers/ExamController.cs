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
    private readonly IExamRepository _repository;

    public ExamController(IExamService service, IExamRepository repository)
    {
        _service = service;
        _repository = repository;
    }

    [HttpGet("restore-test-3")]
    public async Task<IActionResult> RestoreTest3()
    {
        var exam = new ToeicBackend.Domain.Entities.Exam
        {
            Id = "test_3",
            Title = "TOEIC Speaking & Writing - Test 3",
            Description = "Đề TOEIC Speaking & Writing Test 3",
            Difficulty = "medium",
            Duration = 60,
            Year = 2024,
            ImageUrl = "",
            AudioUrl = "",
            IsExam = false,
            IsPractice = true,
            IsPremium = true,
            IsPublished = true,
            ExamType = "full",
            QuestionIds = new List<string>()
        };
        await _repository.AddExamAsync(exam);
        return Ok(new { Message = "Test 3 restored successfully" });
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

    [HttpPost]
    public async Task<ActionResult<ExamDto>> Create([FromBody] SaveExamDto dto)
    {
        var result = await _service.CreateExamAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ExamDto>> Update(string id, [FromBody] UpdateExamDto dto)
    {
        var result = await _service.UpdateExamAsync(id, dto);
        if (result == null) return NotFound(new { Message = "Exam not found" });
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var deleted = await _service.DeleteExamAsync(id);
        if (!deleted) return NotFound(new { Message = "Exam not found or cannot be deleted" });
        return Ok(new { Success = true, Message = "Exam deleted successfully" });
    }
}
