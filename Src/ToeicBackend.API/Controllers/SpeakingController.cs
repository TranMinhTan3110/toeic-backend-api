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
    public async Task<ActionResult<IEnumerable<SpeakingQuestionDto>>> GetByTaskNumber(int taskNumber)
    {
        var results = await _service.GetByTaskNumberAsync(taskNumber);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SpeakingQuestionDto>> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(new { Message = "Speaking question not found" });
        return Ok(result);
    }
}
