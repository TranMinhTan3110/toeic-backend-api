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
    public async Task<IActionResult> GetExercises(string topicId)
    {
        if (string.IsNullOrEmpty(topicId))
        {
            return BadRequest(new { Message = "Topic ID is required" });
        }

        var exercises = await _service.GetExercisesByTopicIdAsync(topicId);
        return Ok(exercises);
    }
}
