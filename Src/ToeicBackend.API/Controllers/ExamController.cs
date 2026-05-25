using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/exam")]
public class ExamController : ControllerBase
{
    private readonly IExamService _service;

    public ExamController(IExamService service)
    {
        _service = service;
    }

    [HttpGet("questions/{examId}")]
    public async Task<ActionResult<IEnumerable<ListeningQuestionDto>>> GetExamQuestions(string examId)
    {
        var results = await _service.GetExamQuestionsAsync(examId);
        return Ok(results);
    }

    [HttpGet("groups/{examId}")]
    public async Task<ActionResult<IEnumerable<ListeningGroupDto>>> GetExamGroups(string examId)
    {
        var results = await _service.GetExamGroupsAsync(examId);
        return Ok(results);
    }
}
