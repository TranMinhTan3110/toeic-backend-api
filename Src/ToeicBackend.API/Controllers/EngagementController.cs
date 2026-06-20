using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Enums;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.API.Extensions;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/engagement")]
[Authorize]
public class EngagementController : ControllerBase
{
    private readonly IEngagementService _engagementService;

    public EngagementController(IEngagementService engagementService)
    {
        _engagementService = engagementService;
    }

    /// <summary>
    /// Ghi nhận hoạt động học và cộng EP.
    /// ActivityType: VocabFlashcardReview | VocabTyping | VocabMatching | VocabSentence
    /// </summary>
    [HttpPost("activity")]
    public async Task<IActionResult> RecordActivity([FromBody] RecordActivityRequestDto dto)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Token không hợp lệ" });

        if (!Enum.TryParse<ActivityType>(dto.ActivityType, ignoreCase: true, out var activityType))
            return BadRequest(new { message = $"ActivityType không hợp lệ: {dto.ActivityType}" });

        var result = await _engagementService.RecordActivityAsync(userId, new RecordActivityRequest
        {
            ActivityType    = activityType,
            ReferenceId     = dto.ReferenceId,
            NewlyMastered   = dto.NewlyMastered,
            CorrectAnswers  = dto.CorrectAnswers,
            TotalAnswers    = dto.TotalAnswers,
        });

        if (result == null)
            return BadRequest(new { message = "ActivityType không được hỗ trợ" });

        return Ok(result);
    }
}

public class RecordActivityRequestDto
{
    public string ActivityType { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public bool NewlyMastered { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalAnswers { get; set; }
}
