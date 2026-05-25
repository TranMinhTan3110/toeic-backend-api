using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Application.DTOs;
using ToeicBackend.API.Extensions;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/vocabularies")]
public class VocabularyController : ControllerBase
{
    private readonly IVocabularyService _service;
    private readonly IVocabularyProgressService _progressService;

    public VocabularyController(IVocabularyService service, IVocabularyProgressService progressService)
    {
        _service = service;
        _progressService = progressService;
    }

    [HttpPost("progress")]
    [Authorize]
    public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressDto dto)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        if (string.IsNullOrEmpty(dto.VocabularyId))
        {
            return BadRequest(new { Message = "VocabularyId is required" });
        }

        if (dto.Quality is < 0 or > 5)
        {
            return BadRequest(new { Message = "Quality must be between 0 and 5" });
        }

        var result = await _progressService.UpdateProgressAsync(userId, dto);
        return Ok(result);
    }

    [HttpGet("user-progress")]
    [Authorize]
    public async Task<IActionResult> GetUserProgress()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var result = await _progressService.GetUserProgressAsync(userId);
        return Ok(result);
    }

    [HttpGet("due")]
    [Authorize]
    public async Task<IActionResult> GetDueVocabularies()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var dueIds = await _progressService.GetDueVocabularyIdsAsync(userId);
        return Ok(dueIds);
    }

    [HttpGet("due/details")]
    [Authorize]
    public async Task<IActionResult> GetDueVocabulariesDetails()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var results = await _progressService.GetDueVocabulariesAsync(userId);
        return Ok(results);
    }

    [HttpGet("hub-stats")]
    [Authorize]
    public async Task<IActionResult> GetHubStats()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var result = await _progressService.GetHubStatsAsync(userId);
        return Ok(result);
    }

    [HttpGet("review-schedule")]
    [Authorize]
    public async Task<IActionResult> GetReviewSchedule()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Token không hợp lệ" });

        var result = await _progressService.GetReviewScheduleAsync(userId);
        return Ok(result);
    }

    [HttpGet("starred")]
    [Authorize]
    public async Task<IActionResult> GetStarredVocabularies()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        var result = await _progressService.GetStarredVocabulariesAsync(userId);
        return Ok(result);
    }

    [HttpPost("toggle-star/{vocabId}")]
    [Authorize]
    public async Task<IActionResult> ToggleStar(string vocabId)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }

        if (string.IsNullOrEmpty(vocabId))
        {
            return BadRequest(new { Message = "Vocabulary ID is required" });
        }

        var result = await _progressService.ToggleStarAsync(userId, vocabId);
        return Ok(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? topic = null, [FromQuery] string? level = null)
    {
        // Lấy userId nếu user đã đăng nhập (optional)
        var userId = User.Identity?.IsAuthenticated == true
            ? User.GetFirebaseUserId()
            : null;

        var results = await _service.GetVocabularyListAsync(topic, level, userId);
        return Ok(results);
    }

    [HttpGet("topics")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTopics()
    {
        var topics = await _service.GetTopicsAsync();
        return Ok(topics);
    }

    [HttpGet("levels")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLevels()
    {
        var levels = await _service.GetLevelsAsync();
        return Ok(levels);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetVocabularyByIdAsync(id);
        if (result == null) return NotFound(new { Message = "Vocabulary not found" });
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateVocabularyDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateVocabularyAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(string id, [FromBody] CreateVocabularyDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateVocabularyAsync(id, dto);
        if (updated == null)
        {
            return NotFound(new { Message = "Không tìm thấy từ vựng để cập nhật" });
        }

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _service.DeleteVocabularyAsync(id);
        if (!success)
        {
            return NotFound(new { Message = "Không tìm thấy từ vựng để xóa" });
        }

        return NoContent();
    }

    [HttpPost("bulk")]
    [Authorize]
    public async Task<IActionResult> BulkCreate([FromBody] List<CreateVocabularyDto> dtos)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (dtos == null || !dtos.Any())
        {
            return BadRequest(new { Message = "Dữ liệu rỗng." });
        }

        var importedCount = await _service.BulkCreateVocabularyAsync(dtos);
        return Ok(new { Message = $"Đã nhập thành công {importedCount} từ vựng mới.", Count = importedCount });
    }
}
