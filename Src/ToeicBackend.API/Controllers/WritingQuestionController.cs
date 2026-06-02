using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.DTOs;
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
    /// Admin list endpoint with cache/fallback handling.
    /// </summary>
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllAdmin()
    {
        try
        {
            var results = await _service.GetAllAsync();
            return Ok(results);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Writing Admin Error] {ex.Message}");
            return Ok(Array.Empty<WritingQuestionDto>());
        }
    }

    /// <summary>
    /// Fast practice counts by task type.
    /// </summary>
    [HttpGet("admin/counts")]
    public async Task<IActionResult> GetPracticeCounts()
    {
        try
        {
            var counts = await _service.GetPracticeCountsByTaskTypeAsync();
            return Ok(counts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Writing Counts Error] {ex.Message}");
            return Ok(new Dictionary<string, int>
            {
                { "write_sentence", 0 },
                { "respond_email", 0 },
                { "opinion_essay", 0 }
            });
        }
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

    [HttpPost("admin")]
    public async Task<IActionResult> AddAdmin([FromBody] WritingQuestionDto dto)
    {
        try
        {
            var validation = ValidateWritingQuestion(dto);
            if (validation != null) return BadRequest(new { success = false, message = validation });

            var id = await _service.AddAsync(dto);
            return Ok(new { success = true, id });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error AddWritingQuestion] {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message, detail = ex.ToString() });
        }
    }

    [HttpPut("admin/{id}")]
    public async Task<IActionResult> UpdateAdmin(string id, [FromBody] WritingQuestionDto dto)
    {
        try
        {
            var validation = ValidateWritingQuestion(dto);
            if (validation != null) return BadRequest(new { success = false, message = validation });

            var updated = await _service.UpdateAsync(id, dto);
            if (!updated) return NotFound(new { success = false, message = "Writing question not found" });
            return Ok(new { success = true, id });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error UpdateWritingQuestion] {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message, detail = ex.ToString() });
        }
    }

    [HttpDelete("admin/{id}")]
    public async Task<IActionResult> DeleteAdmin(string id)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound(new { success = false, message = "Writing question not found" });
            return Ok(new { success = true, message = "Writing question deleted" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error DeleteWritingQuestion] {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message, detail = ex.ToString() });
        }
    }

    private static string? ValidateWritingQuestion(WritingQuestionDto dto)
    {
        var taskType = dto.TaskType?.Trim().ToLowerInvariant();
        if (taskType is not ("write_sentence" or "respond_email" or "opinion_essay"))
            return "TaskType must be write_sentence, respond_email, or opinion_essay.";

        if (string.IsNullOrWhiteSpace(dto.PromptText))
            return "PromptText is required.";

        if (taskType == "write_sentence")
        {
            if (dto.TaskNumber is < 1 or > 5)
                return "Write sentence questions must use task numbers 1-5.";
            if (string.IsNullOrWhiteSpace(dto.PromptImageUrl))
                return "PromptImageUrl is required for write_sentence questions.";
            if (dto.GivenWords == null || dto.GivenWords.Count < 2 || dto.GivenWords.Any(string.IsNullOrWhiteSpace))
                return "Two given words are required for write_sentence questions.";
        }

        if (taskType == "respond_email")
        {
            if (dto.TaskNumber is < 6 or > 7)
                return "Respond email questions must use task numbers 6-7.";
            if (string.IsNullOrWhiteSpace(dto.EmailContent))
                return "EmailContent is required for respond_email questions.";
            if (dto.EmailQuestions == null || dto.EmailQuestions.Count == 0 || dto.EmailQuestions.Any(string.IsNullOrWhiteSpace))
                return "At least one email question is required for respond_email questions.";
        }

        if (taskType == "opinion_essay" && dto.TaskNumber != 8)
            return "Opinion essay questions must use task number 8.";

        return null;
    }
}
