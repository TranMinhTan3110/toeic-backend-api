using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/reading")]
public class ReadingController : ControllerBase
{
    private readonly IReadingService _service;

    public ReadingController(IReadingService service)
    {
        _service = service;
    }

    [HttpGet("part/{part}")]
    public async Task<ActionResult<IEnumerable<ReadingQuestionDto>>> GetByPart(int part)
    {
        var results = await _service.GetQuestionsByPartAsync(part);
        return Ok(results);
    }

    [HttpGet("groups/{part}")]
    public async Task<ActionResult<IEnumerable<ReadingGroupDto>>> GetGroupsByPart(int part)
    {
        var results = await _service.GetGroupsByPartAsync(part);
        return Ok(results);
    }

    [HttpGet("count/{part}")]
    public async Task<ActionResult<object>> GetCountByPart(int part)
    {
        try
        {
            var count = await _service.GetCountByPartAsync(part);
            return Ok(new { part, count, type = part == 5 ? "questions" : "groups" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Count Error] Part {part}: {ex.Message}");
            return Ok(new { part, count = 0, type = part == 5 ? "questions" : "groups" });
        }
    }

    [HttpPost("part5/submit")]
    [HttpPost("part6/submit")]
    [HttpPost("part7/submit")]
    public async Task<ActionResult<ReadingSubmitResultDto>> SubmitAnswers([FromBody] ReadingSubmitDto submitDto)
    {
        try
        {
            // Detect part from current endpoint route path to get corresponding questions for validation
            int part = HttpContext.Request.Path.Value?.Contains("part5") == true ? 5 
                       : HttpContext.Request.Path.Value?.Contains("part6") == true ? 6 
                       : 7;

            List<ReadingQuestionDto> questions = new();
            if (part == 5)
            {
                questions = (await _service.GetQuestionsByPartAsync(part)).ToList();
            }
            else
            {
                var groups = await _service.GetGroupsByPartAsync(part);
                questions = groups.SelectMany(g => g.Questions).ToList();
            }

            var questionMap = questions.ToDictionary(q => q.Id);
            var results = new List<ReadingQuestionResultDto>();
            int correctCount = 0;

            foreach (var answerItem in submitDto.Answers)
            {
                if (questionMap.TryGetValue(answerItem.QuestionId, out var q))
                {
                    bool isCorrect = string.Equals(q.CorrectAnswer?.Trim(), answerItem.Answer?.Trim(), StringComparison.OrdinalIgnoreCase);
                    if (isCorrect) correctCount++;

                    results.Add(new ReadingQuestionResultDto
                    {
                        QuestionId = answerItem.QuestionId,
                        SelectedOption = answerItem.Answer,
                        CorrectAnswer = q.CorrectAnswer,
                        IsCorrect = isCorrect
                    });
                }
            }

            return Ok(new ReadingSubmitResultDto
            {
                CorrectCount = correctCount,
                TotalQuestions = submitDto.Answers.Count,
                Results = results
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Submit Error]: {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<ReadingQuestionDto>>> GetAllAdmin()
    {
        try
        {
            var results = await _service.GetAllQuestionsAdminAsync();
            return Ok(results);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Firebase Quota/Error] {ex.Message}");
            // Return mock data fallback in case Firebase quota is exceeded
            var mockData = new List<ReadingQuestionDto>
            {
                new ReadingQuestionDto { Id = "quota_err_1", Part = 5, QuestionText = "Câu hỏi bị ẩn do Firebase hết hạn mức (Quota Exceeded)", Difficulty = "easy" },
                new ReadingQuestionDto { Id = "quota_err_2", Part = 6, QuestionText = "Vui lòng chờ sang ngày mới hoặc nâng cấp gói Firebase.", Difficulty = "medium" },
                new ReadingQuestionDto { Id = "quota_err_3", Part = 7, QuestionText = "Hoặc dùng mock data này để tiếp tục phát triển UI.", Difficulty = "hard" }
            };
            return Ok(mockData);
        }
    }

    [HttpPost("admin/add-single")]
    public async Task<ActionResult> AddSingleQuestion([FromBody] ReadingQuestionDto dto)
    {
        try
        {
            object? explanationValue = dto.Explanation;
            if (explanationValue is System.Text.Json.JsonElement jsonElement)
            {
                explanationValue = jsonElement.ValueKind switch
                {
                    System.Text.Json.JsonValueKind.String => jsonElement.GetString(),
                    System.Text.Json.JsonValueKind.Number => jsonElement.GetDouble(),
                    System.Text.Json.JsonValueKind.True => true,
                    System.Text.Json.JsonValueKind.False => false,
                    System.Text.Json.JsonValueKind.Null => null,
                    _ => jsonElement.ToString()
                };
            }

            var entity = new ReadingQuestion
            {
                Id = string.IsNullOrEmpty(dto.Id) ? "admin_r_q_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() : dto.Id,
                Part = dto.Part,
                QuestionText = dto.QuestionText,
                ImageUrl = dto.ImageUrl,
                Options = dto.Options ?? new List<string>(),
                CorrectAnswer = dto.CorrectAnswer ?? string.Empty,
                Explanation = explanationValue,
                ExplanationVi = dto.ExplanationVi,
                Script = dto.Script,
                GroupId = dto.GroupId,
                Difficulty = dto.Difficulty ?? "medium",
                IsForExam = false,
                IsForPractice = true,
                Skill = "reading",
                GrammarTopicId = dto.GrammarTopicId
            };

            await _service.AddQuestionAsync(entity);
            return Ok(new { success = true, id = entity.Id });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error AddSingleQuestion] {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message, detail = ex.ToString() });
        }
    }

    [HttpPost("admin/add-group")]
    public async Task<ActionResult> AddGroupQuestion([FromBody] ReadingGroupDto groupDto)
    {
        try
        {
            var groupId = string.IsNullOrEmpty(groupDto.Id) ? "admin_r_group_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() : groupDto.Id;
            
            // Create sub-questions first
            var questionIds = new List<string>();
            foreach (var q in groupDto.Questions)
            {
                var qId = string.IsNullOrEmpty(q.Id) ? "admin_r_q_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Guid.NewGuid().ToString().Substring(0, 4) : q.Id;
                questionIds.Add(qId);
                
                var entity = new ReadingQuestion
                {
                    Id = qId,
                    Part = q.Part,
                    QuestionText = q.QuestionText,
                    Options = q.Options ?? new List<string>(),
                    CorrectAnswer = q.CorrectAnswer ?? string.Empty,
                    Explanation = q.Explanation,
                    ExplanationVi = q.ExplanationVi,
                    Difficulty = q.Difficulty ?? "medium",
                    Skill = "reading",
                    GroupId = groupId,
                    IsForExam = false,
                    IsForPractice = true
                };
                await _service.AddQuestionAsync(entity);
            }

            var groupEntity = new QuestionGroup
            {
                Id = groupId,
                Part = groupDto.Part,
                Script = groupDto.Script,
                PassageText = groupDto.PassageText,
                ImageUrl = groupDto.ImageUrl,
                AudioUrl = "", // not used
                Source = groupDto.Source,
                QuestionCount = groupDto.Questions.Count,
                QuestionIds = questionIds
            };
            
            await _service.AddGroupAsync(groupEntity);
            return Ok(new { success = true, id = groupEntity.Id });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error AddGroupQuestion] {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message, detail = ex.ToString() });
        }
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

    [HttpPut("admin/{id}")]
    public async Task<IActionResult> UpdateQuestion(string id, [FromBody] ReadingQuestionDto dto)
    {
        try
        {
            var result = await _service.UpdateQuestionAsync(id, dto);
            if (!result)
            {
                return NotFound(new { success = false, message = "Không tìm thấy câu hỏi để cập nhật." });
            }
            return Ok(new { success = true, message = "Đã cập nhật câu hỏi thành công." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error UpdateQuestion] {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message, detail = ex.ToString() });
        }
    }
}
