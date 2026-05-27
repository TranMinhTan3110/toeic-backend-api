using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/speaking")]
public class SpeakingController : ControllerBase
{
    private readonly ISpeakingService _service;

    public SpeakingController(ISpeakingService service)
    {
        _service = service;
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
}
