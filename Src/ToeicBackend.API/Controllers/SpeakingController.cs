using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToeicBackend.API.Extensions;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/speaking")]
public class SpeakingController : ControllerBase
{
    private readonly ISpeakingService _service;
    private readonly ISpeakingHistoryService _historyService;

    public SpeakingController(ISpeakingService service, ISpeakingHistoryService historyService)
    {
        _service = service;
        _historyService = historyService;
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

    [HttpGet("exam/{examSetId}")]
    public async Task<ActionResult<IEnumerable<SpeakingQuestionDto>>> GetByExamSetId(string examSetId)
    {
        var results = await _service.GetByExamSetIdAsync(examSetId);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SpeakingQuestionDto>> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(new { Message = "Speaking question not found" });
        return Ok(result);
    }

    /// <summary>Chấm bài nói bằng Gemini — so sánh transcript với bài mẫu trên Firestore.</summary>
    [HttpPost("evaluate")]
    [Authorize]
    public async Task<ActionResult<SpeakingEvaluationDto>> Evaluate(
        [FromForm] string questionId,
        [FromForm] string? transcript,
        [FromForm] int? subQuestionIndex,
        IFormFile? audio = null)
    {
        if (string.IsNullOrWhiteSpace(questionId))
        {
            return BadRequest(new { message = "questionId là bắt buộc" });
        }

        try
        {
            byte[]? audioBytes = null;
            string? mimeType = null;

            if (audio != null && audio.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await audio.CopyToAsync(ms);
                    audioBytes = ms.ToArray();
                }
                mimeType = audio.ContentType;
            }

            var result = await _historyService.EvaluateAsync(
                questionId,
                transcript ?? string.Empty,
                subQuestionIndex,
                audioBytes,
                mimeType);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi chấm điểm AI", detail = ex.Message });
        }
    }

    [HttpPost("history")]
    [Authorize]
    public async Task<IActionResult> SaveHistory([FromBody] SaveSpeakingHistoryRequestDto dto)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var historyId = await _historyService.SaveHistoryAsync(userId, dto);
        return Ok(new { success = true, id = historyId });
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<SpeakingHistoryDto>>> GetUserHistory(
        [FromQuery] string? sessionType)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var results = await _historyService.GetUserHistoryAsync(userId, sessionType);
        return Ok(results);
    }

    [HttpGet("history/{id}")]
    [Authorize]
    public async Task<ActionResult<SpeakingHistoryDto>> GetHistoryById(string id)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var result = await _historyService.GetHistoryByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { message = "Không tìm thấy lịch sử với ID này" });
        }

        if (result.UserId != userId)
        {
            return Forbid();
        }

        return Ok(result);
    }

    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<SpeakingQuestionDto>>> GetAllAdmin()
    {
        try
        {
            var results = await _service.GetAllAsync();
            return Ok(results);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Firebase Quota/Error] {ex.Message}");
            var mockData = new List<SpeakingQuestionDto>
            {
                new SpeakingQuestionDto 
                { 
                    Id = "quota_err_spk_1", 
                    TaskNumber = 1, 
                    TaskType = "Read Aloud", 
                    PromptText = "The personnel managers have decided to make staff changes in the finance department. This is expected to be finalized by early next month.", 
                    Difficulty = "easy",
                    IsPractice = true
                },
                new SpeakingQuestionDto 
                { 
                    Id = "quota_err_spk_2", 
                    TaskNumber = 2, 
                    TaskType = "Describe Picture", 
                    PromptText = "Describe this classroom scene.", 
                    Difficulty = "medium",
                    IsPractice = true
                }
            };
            return Ok(mockData);
        }
    }

    [HttpPost("admin/add")]
    public async Task<ActionResult> AddQuestion([FromBody] SpeakingQuestionDto dto)
    {
        try
        {
            var entity = new ToeicBackend.Domain.Entities.SpeakingQuestion
            {
                Id = string.IsNullOrEmpty(dto.Id) ? "admin_spk_q_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() : dto.Id,
                TaskType = dto.TaskType ?? string.Empty,
                TaskNumber = dto.TaskNumber,
                PromptText = dto.PromptText ?? string.Empty,
                PromptImageUrl = dto.PromptImageUrl,
                PromptAudioUrl = dto.PromptAudioUrl,
                ImageUrl = dto.ImageUrl,
                AudioUrl = dto.AudioUrl,
                PreparationTime = dto.PreparationTime,
                ResponseTime = dto.ResponseTime,
                Difficulty = dto.Difficulty ?? "medium",
                AiPrompt = dto.AiPrompt ?? string.Empty,
                ScoringCriteria = dto.ScoringCriteria ?? new List<string>(),
                ExamSetId = dto.ExamSetId,
                Topic = dto.Topic,
                IsPractice = dto.IsPractice,
                IsExam = dto.IsExam,
                MaxScore = dto.MaxScore,
                SampleAnswer = dto.SampleAnswer,
                Questions = dto.Questions ?? new List<string>(),
                AnswerTimes = dto.AnswerTimes ?? new List<int>(),
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Explanation != null)
            {
                entity.Explanation = new ToeicBackend.Domain.Entities.SpeakingExplanation
                {
                    Translation = dto.Explanation.Translation,
                    ContextTranslation = dto.Explanation.ContextTranslation,
                    QuestionsTranslation = dto.Explanation.QuestionsTranslation ?? new List<string>(),
                    SampleAnswers = dto.Explanation.SampleAnswers ?? new List<string>(),
                    SampleAnswersTranslation = dto.Explanation.SampleAnswersTranslation ?? new List<string>(),
                    Keywords = dto.Explanation.Keywords?.Select(k => new ToeicBackend.Domain.Entities.ExplanationKeyword
                    {
                        Word = k.Word,
                        Ipa = k.Ipa,
                        Meaning = k.Meaning
                    }).ToList() ?? new List<ToeicBackend.Domain.Entities.ExplanationKeyword>()
                };
            }

            await _service.AddQuestionAsync(entity);
            return Ok(new { success = true, id = entity.Id });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error AddQuestion] {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message, detail = ex.ToString() });
        }
    }

    [HttpDelete("admin/{id}")]
    public async Task<IActionResult> DeleteQuestion(string id)
    {
        try
        {
            var result = await _service.DeleteQuestionAsync(id);
            if (!result)
            {
                return NotFound(new { success = false, message = "Không tìm thấy câu hỏi." });
            }
            return Ok(new { success = true, message = "Đã xóa câu hỏi thành công." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error DeleteQuestion] {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
