using Microsoft.AspNetCore.Mvc;
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
