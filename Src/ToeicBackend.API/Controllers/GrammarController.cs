using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Application.DTOs;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/grammar")]
[Authorize]
public class GrammarController : ControllerBase
{
    private readonly IGrammarService _service;

    public GrammarController(IGrammarService service)
    {
        _service = service;
    }

    [HttpGet("topics")]
    public async Task<IActionResult> GetTopics()
    {
        var topics = await _service.GetTopicsAsync();
        return Ok(topics);
    }

    [HttpGet("lessons/{topicId}")]
    public async Task<IActionResult> GetLesson(string topicId)
    {
        if (string.IsNullOrEmpty(topicId))
        {
            return BadRequest(new { Message = "Topic ID is required" });
        }

        var lesson = await _service.GetLessonByTopicIdAsync(topicId);
        if (lesson == null)
        {
            return NotFound(new { Message = $"Grammar lesson for topic '{topicId}' not found" });
        }

        return Ok(lesson);
    }

    [HttpGet("exercises/{topicId}")]
    public async Task<IActionResult> GetExercises(string topicId, [FromQuery] bool random = true, [FromQuery] int limit = 0)
    {
        if (string.IsNullOrEmpty(topicId))
        {
            return BadRequest(new { Message = "Topic ID is required" });
        }

        var exercises = await _service.GetExercisesByTopicIdAsync(topicId, random, limit);
        return Ok(exercises);
    }

    [HttpPost("topics")]
    public async Task<IActionResult> CreateTopic([FromBody] CreateGrammarTopicDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _service.CreateTopicAsync(dto);
        return CreatedAtAction(nameof(GetTopics), new { id = created.Id }, created);
    }

    [HttpPut("topics/{id}")]
    public async Task<IActionResult> UpdateTopic(string id, [FromBody] CreateGrammarTopicDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _service.UpdateTopicAsync(id, dto);
        if (updated == null) return NotFound(new { Message = "Topic not found" });

        return Ok(updated);
    }

    [HttpDelete("topics/{id}")]
    public async Task<IActionResult> DeleteTopic(string id)
    {
        var success = await _service.DeleteTopicAsync(id);
        if (!success) return NotFound(new { Message = "Topic not found" });

        return Ok(new { Success = true });
    }

    [HttpPut("lessons/{topicId}")]
    public async Task<IActionResult> SaveLesson(string topicId, [FromBody] CreateGrammarLessonDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var saved = await _service.SaveLessonAsync(topicId, dto);
        return Ok(saved);
    }

    [HttpPost("exercises/{topicId}")]
    public async Task<IActionResult> AddExercise(string topicId, [FromBody] ListeningQuestionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var added = await _service.AddExerciseAsync(topicId, dto);
        return CreatedAtAction(nameof(GetExercises), new { topicId }, added);
    }

    [HttpPut("exercises/{id}")]
    public async Task<IActionResult> UpdateExercise(string id, [FromBody] ListeningQuestionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _service.UpdateExerciseAsync(id, dto);
        if (updated == null) return NotFound(new { Message = "Exercise not found" });

        return Ok(updated);
    }

    [HttpDelete("exercises/{id}")]
    public async Task<IActionResult> DeleteExercise(string id)
    {
        var success = await _service.DeleteExerciseAsync(id);
        if (!success) return NotFound(new { Message = "Exercise not found" });

        return Ok(new { Success = true });
    }
}
