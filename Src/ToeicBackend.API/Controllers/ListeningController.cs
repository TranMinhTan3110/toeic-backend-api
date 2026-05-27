using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ToeicBackend.API.Extensions;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/listening")]
public class ListeningController : ControllerBase
{
    private readonly IListeningService _service;

    public ListeningController(IListeningService service)
    {
        _service = service;
    }

    [HttpGet("part/{part}")]
    public async Task<ActionResult<IEnumerable<ListeningQuestionDto>>> GetByPart(int part)
    {
        var results = await _service.GetQuestionsByPartAsync(part);
        return Ok(results);
    }

    [HttpGet("groups/{part}")]
    public async Task<ActionResult<IEnumerable<ListeningGroupDto>>> GetGroupsByPart(int part)
    {
        var results = await _service.GetGroupsByPartAsync(part);
        return Ok(results);
    }

    /// <summary>
    /// Trả về số lượng câu/nhóm của 1 part — cực nhanh (1 Firestore read).
    /// Dùng cho DetailScreen để hiển thị số câu mà không cần load toàn bộ data.
    /// </summary>
    [HttpGet("count/{part}")]
    public async Task<ActionResult<object>> GetCountByPart(int part)
    {
        try
        {
            var count = await _service.GetCountByPartAsync(part);
            return Ok(new { part, count, type = part <= 2 ? "questions" : "groups" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Count Error] Part {part}: {ex.Message}");
            return Ok(new { part, count = 0, type = part <= 2 ? "questions" : "groups" });
        }
    }

    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<ListeningQuestionDto>>> GetAllAdmin()
    {
        try
        {
            var results = await _service.GetAllQuestionsAdminAsync();
            return Ok(results);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Firebase Quota/Error] {ex.Message}");
            var mockData = new List<ListeningQuestionDto>
            {
                new ListeningQuestionDto { Id = "quota_err_1", Part = 1, QuestionText = "Câu hỏi bị ẩn do Firebase hết hạn mức (Quota Exceeded)", Difficulty = "easy", Script = "Quota Exceeded Mock" },
                new ListeningQuestionDto { Id = "quota_err_2", Part = 2, QuestionText = "Vui lòng chờ sang ngày mới hoặc nâng cấp gói Firebase.", Difficulty = "medium", Script = "Quota Exceeded Mock" },
                new ListeningQuestionDto { Id = "quota_err_3", Part = 3, QuestionText = "Hoặc dùng mock data này để tiếp tục phát triển UI.", Difficulty = "hard", Script = "Quota Exceeded Mock" },
                new ListeningQuestionDto { Id = "quota_err_4", Part = 4, QuestionText = "Tính năng thêm câu hỏi (POST) vẫn có thể hoạt động.", Difficulty = "medium", Script = "Quota Exceeded Mock" }
            };
            return Ok(mockData);
        }
    }

    [HttpPost("admin/add-single")]
    public async Task<ActionResult> AddSingleQuestion([FromBody] ListeningQuestionDto dto)
    {
        var entity = new ToeicBackend.Domain.Entities.ListeningQuestion
        {
            Id = string.IsNullOrEmpty(dto.Id) ? "admin_p_q_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() : dto.Id,
            Part = dto.Part,
            QuestionText = dto.QuestionText,
            ImageUrl = dto.ImageUrl,
            AudioUrl = dto.AudioUrl,
            Options = dto.Options,
            CorrectAnswer = dto.CorrectAnswer,
            Explanation = dto.Explanation,
            ExplanationVi = dto.ExplanationVi,
            Script = dto.Script,
            GroupId = dto.GroupId,
            Difficulty = dto.Difficulty ?? "medium",
            IsForExam = false,
            IsForPractice = true,
            Skill = "listening"
        };
        await _service.AddQuestionAsync(entity);
        return Ok(new { success = true, id = entity.Id });
    }

    [HttpPost("admin/add-group")]
    public async Task<ActionResult> AddGroupQuestion([FromBody] ListeningGroupDto groupDto)
    {
        var groupId = string.IsNullOrEmpty(groupDto.Id) ? "admin_group_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() : groupDto.Id;
        
        // Tạo các câu hỏi con
        var questionIds = new List<string>();
        foreach(var q in groupDto.Questions)
        {
            var qId = string.IsNullOrEmpty(q.Id) ? "admin_p_q_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Guid.NewGuid().ToString().Substring(0, 4) : q.Id;
            questionIds.Add(qId);
            var entity = new ToeicBackend.Domain.Entities.ListeningQuestion
            {
                Id = qId,
                Part = q.Part,
                QuestionText = q.QuestionText,
                Options = q.Options,
                CorrectAnswer = q.CorrectAnswer,
                Difficulty = q.Difficulty ?? "medium",
                Skill = "listening",
                GroupId = groupId, // set GroupId just in case
                IsForExam = false,
                IsForPractice = true
            };
            await _service.AddQuestionAsync(entity);
        }

        var groupEntity = new ToeicBackend.Domain.Entities.QuestionGroup
        {
            Id = groupId,
            Part = groupDto.Part,
            Script = groupDto.Script,
            PassageText = groupDto.PassageText,
            ImageUrl = groupDto.ImageUrl,
            AudioUrl = groupDto.AudioUrl,
            Source = groupDto.Source,
            QuestionCount = groupDto.Questions.Count,
            QuestionIds = questionIds
        };
        
        await _service.AddGroupAsync(groupEntity);
        
        return Ok(new { success = true, id = groupEntity.Id });
    }

    // --- History Practice Endpoints ---

    [HttpPost("history")]
    [Authorize]
    public async Task<IActionResult> SaveHistory([FromBody] SaveListeningHistoryRequestDto dto)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Token không hợp lệ" });

        var historyId = await _service.SaveHistoryAsync(userId, dto);
        return Ok(new { success = true, id = historyId });
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ListeningHistoryDto>>> GetUserHistory()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Token không hợp lệ" });

        var results = await _service.GetUserHistoryAsync(userId);
        return Ok(results);
    }

    [HttpGet("history/{id}")]
    [Authorize]
    public async Task<ActionResult<ListeningHistoryDto>> GetHistoryById(string id)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Token không hợp lệ" });

        var result = await _service.GetHistoryByIdAsync(id);
        if (result == null)
            return NotFound(new { message = "Không tìm thấy lịch sử với ID này" });

        if (result.UserId != userId)
            return Forbid();

        return Ok(result);
    }

    [HttpDelete("admin/{id}")]
    public async Task<IActionResult> DeleteQuestion(string id)
    {
        var result = await _service.DeleteQuestionAsync(id);
        if (!result)
        {
            return NotFound(new { message = "Không tìm thấy câu hỏi." });
        }
        return Ok(new { success = true, message = "Đã xóa câu hỏi thành công." });
    }
}

