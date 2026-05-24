using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AnalyzeRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Sentence))
        {
            return BadRequest(new { message = "Câu văn không được để trống" });
        }

        try
        {
            var result = await _aiService.AnalyzeSentenceAsync(request.Sentence, request.Word, request.Situation);
            return Ok(new { result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống AI", detail = ex.Message });
        }
    }

    [HttpGet("scenario")]
    public async Task<IActionResult> GetScenario([FromQuery] string word, [FromQuery] string meaning)
    {
        if (string.IsNullOrEmpty(word))
        {
            return BadRequest(new { message = "Từ vựng không được để trống" });
        }

        try
        {
            var result = await _aiService.GenerateScenarioAsync(word, meaning);
            return Ok(new { result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống AI khi tạo tình huống", detail = ex.Message });
        }
    }
}

public class AnalyzeRequest
{
    public string Sentence { get; set; } = string.Empty;
    public string Word { get; set; } = string.Empty;
    public string Situation { get; set; } = string.Empty;
}
