using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Application.DTOs;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/vocabularies")]
public class VocabularyController : ControllerBase
{
    private readonly IVocabularyService _service;

    public VocabularyController(IVocabularyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? topic = null, [FromQuery] string? level = null)
    {
        Console.WriteLine($"[DEBUG] GetAll called with topic: '{topic}', level: '{level}'");
        var results = await _service.GetVocabularyListAsync(topic, level);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetVocabularyByIdAsync(id);
        if (result == null) return NotFound(new { Message = "Vocabulary not found" });
        return Ok(result);
    }

    [HttpGet("topics")]
    public async Task<IActionResult> GetTopics()
    {
        var topics = await _service.GetTopicsAsync();
        return Ok(topics);
    }

    [HttpGet("levels")]
    public async Task<IActionResult> GetLevels()
    {
        var levels = await _service.GetLevelsAsync();
        return Ok(levels);
    }
}
