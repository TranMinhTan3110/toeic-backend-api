using Microsoft.AspNetCore.Mvc;
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
}
