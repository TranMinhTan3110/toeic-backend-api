using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/writing-questions")]
public class WritingQuestionController : ControllerBase
{
    private readonly IWritingQuestionService _service;

    public WritingQuestionController(IWritingQuestionService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all writing questions
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        Console.WriteLine($"[DEBUG] GetAll writing questions called");
        var results = await _service.GetAllAsync();
        return Ok(results);
    }

    /// <summary>
    /// Get writing question by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(new { Message = "Writing question not found" });
        return Ok(result);
    }

    /// <summary>
    /// Get questions by task type (write_sentence, respond_email, opinion_essay)
    /// </summary>
    [HttpGet("by-type/{taskType}")]
    public async Task<IActionResult> GetByTaskType(string taskType)
    {
        Console.WriteLine($"[DEBUG] GetByTaskType called with taskType: '{taskType}'");
        var results = await _service.GetByTaskTypeAsync(taskType);
        return Ok(results);
    }

    /// <summary>
    /// Get questions by task number (1-8)
    /// </summary>
    [HttpGet("by-number/{taskNumber}")]
    public async Task<IActionResult> GetByTaskNumber(int taskNumber)
    {
        Console.WriteLine($"[DEBUG] GetByTaskNumber called with taskNumber: {taskNumber}");
        var results = await _service.GetByTaskNumberAsync(taskNumber);
        return Ok(results);
    }

    /// <summary>
    /// Get questions by difficulty (easy, medium, hard)
    /// </summary>
    [HttpGet("by-difficulty/{difficulty}")]
    public async Task<IActionResult> GetByDifficulty(string difficulty)
    {
        Console.WriteLine($"[DEBUG] GetByDifficulty called with difficulty: '{difficulty}'");
        var results = await _service.GetByDifficultyAsync(difficulty);
        return Ok(results);
    }

    /// <summary>
    /// Get practice questions only (is_practice = true)
    /// </summary>
    [HttpGet("practice")]
    public async Task<IActionResult> GetPracticeQuestions()
    {
        Console.WriteLine($"[DEBUG] GetPracticeQuestions called");
        var results = await _service.GetPracticeQuestionsAsync();
        return Ok(results);
    }

    /// <summary>
    /// Get practice questions by task type
    /// </summary>
    [HttpGet("by-type/{taskType}/practice")]
    public async Task<IActionResult> GetPracticeByTaskType(string taskType)
    {
        Console.WriteLine($"[DEBUG] GetPracticeByTaskType called with taskType: '{taskType}'");
        var results = await _service.GetPracticeByTaskTypeAsync(taskType);
        return Ok(results);
    }

    /// <summary>
    /// Get exam questions by task type
    /// </summary>
    [HttpGet("by-type/{taskType}/exam")]
    public async Task<IActionResult> GetExamByTaskType(string taskType)
    {
        Console.WriteLine($"[DEBUG] GetExamByTaskType called with taskType: '{taskType}'");
        var results = await _service.GetExamByTaskTypeAsync(taskType);
        return Ok(results);
    }

    /// <summary>
    /// Get available task types
    /// </summary>
    [HttpGet("types")]
    public async Task<IActionResult> GetAvailableTaskTypes()
    {
        var types = await _service.GetAvailableTaskTypesAsync();
        return Ok(types);
    }

    /// <summary>
    /// Get writing questions by exam set ID
    /// </summary>
    [HttpGet("exam/{examSetId}")]
    public async Task<IActionResult> GetQuestionsByExamSetId(string examSetId)
    {
        Console.WriteLine($"[DEBUG] GetQuestionsByExamSetId called with examSetId: '{examSetId}'");
        var results = await _service.GetQuestionsByExamSetIdAsync(examSetId);
        return Ok(results);
    }
}
