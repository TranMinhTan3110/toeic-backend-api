using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        if (!string.IsNullOrEmpty(topic) && !string.IsNullOrEmpty(level))
        {
            var results = await _service.GetVocabularyByTopicAndLevelAsync(topic, level);
            return Ok(results);
        }
        
        if (!string.IsNullOrEmpty(topic))
        {
            var results = await _service.GetVocabularyByTopicAsync(topic);
            return Ok(results);
        }

        var all = await _service.GetAllVocabularyAsync();
        return Ok(all);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetVocabularyByIdAsync(id);
        if (result == null) return NotFound(new { Message = "Vocabulary not found" });
        return Ok(result);
    }

    [HttpGet("topic/{topic}")]
    public async Task<IActionResult> GetByTopic(string topic)
    {
        var results = await _service.GetVocabularyByTopicAsync(topic);
        return Ok(results);
    }
}
