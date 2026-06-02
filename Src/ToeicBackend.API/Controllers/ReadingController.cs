using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.Common;
using ToeicBackend.Application.DTOs.Reading;
using ToeicBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using ToeicBackend.API.Extensions;
using ToeicBackend.Application.DTOs;

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

    [HttpGet("part5/questions")]
    public async Task<IActionResult> GetPart5Questions([FromQuery] int? count = null)
    {
        // if count is null or not provided, service will return all available questions
        var items = await _service.GetPart5QuestionsAsync(count);
        var resp = new ApiResponse<IEnumerable<Part5QuestionDto>> { Data = items };
        return Ok(resp);
    }

    [HttpPost("part5/submit")]
    public async Task<IActionResult> SubmitPart5([FromBody] System.Text.Json.JsonElement body)
    {
        // Normalize different possible client payload shapes into Part5SubmitRequestDto
        var answers = new Dictionary<string, string>();

        try
        {
            if (body.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                // Case 1: { "answers": { "q1": "A", ... } }
                if (body.TryGetProperty("answers", out var answersProp))
                {
                    // answers may be an object map or an array
                    if (answersProp.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        foreach (var prop in answersProp.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                answers[prop.Name] = prop.Value.GetString() ?? string.Empty;
                            }
                        }
                    }
                    else if (answersProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        // array of items: allow { questionId, selectedOption } or { questionId, selectedIndex }
                        foreach (var item in answersProp.EnumerateArray())
                        {
                            if (item.ValueKind != System.Text.Json.JsonValueKind.Object) continue;

                            string? qid = null;
                            string? sel = null;

                            if (item.TryGetProperty("questionId", out var q1)) qid = q1.GetString();
                            if (item.TryGetProperty("question_id", out var q2)) qid = qid ?? q2.GetString();
                            if (item.TryGetProperty("id", out var q3)) qid = qid ?? q3.GetString();

                            if (item.TryGetProperty("selectedOption", out var s1) && s1.ValueKind == System.Text.Json.JsonValueKind.String) sel = s1.GetString();
                            if (item.TryGetProperty("selected_option", out var s2) && s2.ValueKind == System.Text.Json.JsonValueKind.String) sel = sel ?? s2.GetString();
                            if (item.TryGetProperty("answer", out var s3) && s3.ValueKind == System.Text.Json.JsonValueKind.String) sel = sel ?? s3.GetString();
                            if (item.TryGetProperty("selected", out var s4) && s4.ValueKind == System.Text.Json.JsonValueKind.String) sel = sel ?? s4.GetString();

                            // support numeric selectedIndex
                            if (sel == null)
                            {
                                if (item.TryGetProperty("selectedIndex", out var si) && si.ValueKind == System.Text.Json.JsonValueKind.Number)
                                {
                                    if (si.TryGetInt32(out var idx)) sel = IndexToOption(idx);
                                }
                                else if (item.TryGetProperty("selected_index", out var si2) && si2.ValueKind == System.Text.Json.JsonValueKind.Number)
                                {
                                    if (si2.TryGetInt32(out var idx2)) sel = IndexToOption(idx2);
                                }
                            }

                            if (!string.IsNullOrEmpty(qid) && !string.IsNullOrEmpty(sel)) answers[qid!] = sel!;
                        }
                    }
                }
                else
                {
                    // Case 2: top-level map { "q1": "A", ... }
                    bool looksLikeMap = true;
                    foreach (var prop in body.EnumerateObject())
                    {
                        if (prop.Value.ValueKind != System.Text.Json.JsonValueKind.String)
                        {
                            looksLikeMap = false;
                            break;
                        }
                    }

                    if (looksLikeMap)
                    {
                        foreach (var prop in body.EnumerateObject())
                        {
                            answers[prop.Name] = prop.Value.GetString() ?? string.Empty;
                        }
                    }
                }
            }
            else if (body.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                // Case 3: array of items [{ "questionId":"q1", "selectedOption":"A" }, ...]
                foreach (var item in body.EnumerateArray())
                {
                    if (item.ValueKind != System.Text.Json.JsonValueKind.Object) continue;

                    string? qid = null;
                    string? sel = null;

                    if (item.TryGetProperty("questionId", out var q1)) qid = q1.GetString();
                    if (item.TryGetProperty("question_id", out var q2)) qid = qid ?? q2.GetString();
                    if (item.TryGetProperty("id", out var q3)) qid = qid ?? q3.GetString();

                    if (item.TryGetProperty("selectedOption", out var s1)) sel = s1.GetString();
                    if (item.TryGetProperty("selected_option", out var s2)) sel = sel ?? s2.GetString();
                    if (item.TryGetProperty("answer", out var s3)) sel = sel ?? s3.GetString();
                    if (item.TryGetProperty("selected", out var s4)) sel = sel ?? s4.GetString();

                    if (!string.IsNullOrEmpty(qid) && !string.IsNullOrEmpty(sel)) answers[qid!] = sel!;
                }
            }
        }
        catch
        {
            return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid request payload" });
        }

        if (answers.Count == 0)
        {
            return BadRequest(new ApiResponse<string> { Success = false, Message = "No answers provided" });
        }

        var request = new Part5SubmitRequestDto { Answers = answers };
        var result = await _service.SubmitPart5AnswersAsync(request);
        var resp = new ApiResponse<Part5SubmitResponseDto> { Data = result };
        return Ok(resp);
    }

    private static string IndexToOption(int idx)
    {
        return idx switch
        {
            0 => "A",
            1 => "B",
            2 => "C",
            3 => "D",
            _ => string.Empty
        };
    }

    private Dictionary<string, string>? ParseAnswers(System.Text.Json.JsonElement body)
    {
        var answers = new Dictionary<string, string>();
        try
        {
            if (body.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                if (body.TryGetProperty("answers", out var answersProp))
                {
                    if (answersProp.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        foreach (var prop in answersProp.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                                answers[prop.Name] = prop.Value.GetString() ?? string.Empty;
                            else if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Number)
                            {
                                if (prop.Value.TryGetInt32(out var idx))
                                {
                                    var opt = IndexToOption(idx);
                                    if (!string.IsNullOrEmpty(opt)) answers[prop.Name] = opt;
                                }
                            }
                        }
                    }
                    else if (answersProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var item in answersProp.EnumerateArray())
                        {
                            if (item.ValueKind != System.Text.Json.JsonValueKind.Object) continue;
                            string? qid = null;
                            string? sel = null;
                            if (item.TryGetProperty("questionId", out var q1)) qid = q1.GetString();
                            if (item.TryGetProperty("question_id", out var q2)) qid = qid ?? q2.GetString();
                            if (item.TryGetProperty("id", out var q3)) qid = qid ?? q3.GetString();
                            if (item.TryGetProperty("selectedOption", out var s1) && s1.ValueKind == System.Text.Json.JsonValueKind.String) sel = s1.GetString();
                            if (item.TryGetProperty("selected_option", out var s2) && s2.ValueKind == System.Text.Json.JsonValueKind.String) sel = sel ?? s2.GetString();
                            if (item.TryGetProperty("answer", out var s3) && s3.ValueKind == System.Text.Json.JsonValueKind.String) sel = sel ?? s3.GetString();
                            if (item.TryGetProperty("selected", out var s4) && s4.ValueKind == System.Text.Json.JsonValueKind.String) sel = sel ?? s4.GetString();
                            if (sel == null)
                            {
                                if (item.TryGetProperty("selectedIndex", out var si) && si.ValueKind == System.Text.Json.JsonValueKind.Number)
                                {
                                    if (si.TryGetInt32(out var idx)) sel = IndexToOption(idx);
                                }
                                else if (item.TryGetProperty("selected_index", out var si2) && si2.ValueKind == System.Text.Json.JsonValueKind.Number)
                                {
                                    if (si2.TryGetInt32(out var idx2)) sel = IndexToOption(idx2);
                                }
                            }
                            if (!string.IsNullOrEmpty(qid) && !string.IsNullOrEmpty(sel)) answers[qid!] = sel!;
                        }
                    }
                }
                else
                {
                    bool looksLikeMap = true;
                    foreach (var prop in body.EnumerateObject())
                    {
                        if (prop.Value.ValueKind != System.Text.Json.JsonValueKind.String)
                        {
                            looksLikeMap = false;
                            break;
                        }
                    }
                    if (looksLikeMap)
                    {
                        foreach (var prop in body.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                                answers[prop.Name] = prop.Value.GetString() ?? string.Empty;
                            else if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Number)
                            {
                                if (prop.Value.TryGetInt32(out var idx))
                                {
                                    var opt = IndexToOption(idx);
                                    if (!string.IsNullOrEmpty(opt)) answers[prop.Name] = opt;
                                }
                            }
                        }
                    }
                }
            }
            else if (body.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in body.EnumerateArray())
                {
                    if (item.ValueKind != System.Text.Json.JsonValueKind.Object) continue;
                    string? qid = null;
                    string? sel = null;
                    if (item.TryGetProperty("questionId", out var q1)) qid = q1.GetString();
                    if (item.TryGetProperty("question_id", out var q2)) qid = qid ?? q2.GetString();
                    if (item.TryGetProperty("id", out var q3)) qid = qid ?? q3.GetString();
                    if (item.TryGetProperty("selectedOption", out var s1) && s1.ValueKind == System.Text.Json.JsonValueKind.String) sel = s1.GetString();
                    if (item.TryGetProperty("selected_option", out var s2) && s2.ValueKind == System.Text.Json.JsonValueKind.String) sel = sel ?? s2.GetString();
                    if (item.TryGetProperty("answer", out var s3) && s3.ValueKind == System.Text.Json.JsonValueKind.String) sel = sel ?? s3.GetString();
                    if (item.TryGetProperty("selected", out var s4) && s4.ValueKind == System.Text.Json.JsonValueKind.String) sel = sel ?? s4.GetString();

                    // numeric selectedIndex in top-level array
                    if (sel == null)
                    {
                        if (item.TryGetProperty("selectedIndex", out var si) && si.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            if (si.TryGetInt32(out var idx)) sel = IndexToOption(idx);
                        }
                        else if (item.TryGetProperty("selected_index", out var si2) && si2.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            if (si2.TryGetInt32(out var idx2)) sel = IndexToOption(idx2);
                        }
                    }
                    if (!string.IsNullOrEmpty(qid) && !string.IsNullOrEmpty(sel)) answers[qid!] = sel!;
                }
            }
        }
        catch
        {
            return null;
        }

        return answers;
    }

    [HttpGet("part6/questions")]
    public async Task<IActionResult> GetPart6Questions()
    {
        var groups = await _service.GetPart6PassagesAsync();
        var resp = new ApiResponse<IEnumerable<Part6PassageDto>> { Data = groups };
        return Ok(resp);
    }

    [HttpPost("part6/submit")]
    public async Task<IActionResult> SubmitPart6([FromBody] System.Text.Json.JsonElement body)
    {
        var answers = ParseAnswers(body);
        if (answers == null) return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid request payload" });
        if (answers.Count == 0) return BadRequest(new ApiResponse<string> { Success = false, Message = "No answers provided" });

        var request = new Part6SubmitRequestDto { Answers = answers };
        var result = await _service.SubmitPart6AnswersAsync(request);
        var resp = new ApiResponse<Part6SubmitResponseDto> { Data = result };
        return Ok(resp);
    }

    [HttpGet("part7/questions")]
    public async Task<IActionResult> GetPart7Questions()
    {
        var groups = await _service.GetPart7PassagesAsync();
        var resp = new ApiResponse<IEnumerable<Part7PassageDto>> { Data = groups };
        return Ok(resp);
    }

    // --- History Practice Endpoints for Reading ---

    [HttpPost("history")]
    [Authorize]
    public async Task<IActionResult> SaveHistory([FromBody] SaveReadingHistoryRequestDto dto)
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Token không hợp lệ" });

        var historyId = await _service.SaveHistoryAsync(userId, dto);
        return Ok(new { success = true, id = historyId });
    }

    // compatibility aliases: some frontends call part-specific history routes
    [HttpPost("part5/history")]
    [Authorize]
    public Task<IActionResult> SaveHistory_Part5([FromBody] SaveReadingHistoryRequestDto dto)
    {
        if (dto == null) return Task.FromResult<IActionResult>(BadRequest(new ApiResponse<string> { Success = false, Message = "Request body cannot be empty" }));
        dto.Part = 5;
        return SaveHistory(dto);
    }

    [HttpPost("part/{part}/history")]
    [Authorize]
    public Task<IActionResult> SaveHistory_Part([FromRoute] int part, [FromBody] SaveReadingHistoryRequestDto dto)
    {
        if (dto == null) return Task.FromResult<IActionResult>(BadRequest(new ApiResponse<string> { Success = false, Message = "Request body cannot be empty" }));
        dto.Part = part;
        return SaveHistory(dto);
    }

    [HttpPost("part6/history")]
    [Authorize]
    public Task<IActionResult> SaveHistory_Part6([FromBody] SaveReadingHistoryRequestDto dto)
    {
        if (dto == null) return Task.FromResult<IActionResult>(BadRequest(new ApiResponse<string> { Success = false, Message = "Request body cannot be empty" }));
        dto.Part = 6;
        return SaveHistory(dto);
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReadingHistoryDto>>> GetUserHistory()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Token không hợp lệ" });

        var results = await _service.GetUserHistoryAsync(userId);
        return Ok(results);
    }

    [HttpGet("part5/history")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReadingHistoryDto>>> GetPart5UserHistory()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Token không hợp lệ" });

        var results = await _service.GetPart5HistoryAsync(userId);
        return Ok(results);
    }

    [HttpGet("part6/history")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReadingHistoryDto>>> GetPart6UserHistory()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Token không hợp lệ" });

        var results = await _service.GetPart6HistoryAsync(userId);
        return Ok(results);
    }

    [HttpGet("history/{id}")]
    [Authorize]
    public async Task<ActionResult<ReadingHistoryDto>> GetHistoryById(string id)
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

    [HttpPost("part7/submit")]
    public async Task<IActionResult> SubmitPart7([FromBody] System.Text.Json.JsonElement body)
    {
        var answers = ParseAnswers(body);
        if (answers == null) return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid request payload" });
        if (answers.Count == 0) return BadRequest(new ApiResponse<string> { Success = false, Message = "No answers provided" });
        var request = new Part7SubmitRequestDto { Answers = answers };
        var result = await _service.SubmitPart7AnswersAsync(request);
        var resp = new ApiResponse<Part7SubmitResponseDto> { Data = result };
        return Ok(resp);
    }

    [HttpPost("part7/history")]
    [Authorize]
    public async Task<IActionResult> SaveHistory_Part7([FromBody] SaveReadingHistoryRequestDto dto)
    {
        if (dto == null) return BadRequest(new ApiResponse<string> { Success = false, Message = "Request body cannot be empty" });
        dto.Part = 7;
        return await SaveHistory(dto);
    }

    [HttpGet("part7/history")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReadingHistoryDto>>> GetPart7UserHistory()
    {
        var userId = User.GetFirebaseUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Token không hợp lệ" });

        var results = await _service.GetPart7HistoryAsync(userId);
        return Ok(results);
    }
}
