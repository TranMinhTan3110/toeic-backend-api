using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    [HttpGet("weekly")]
    [AllowAnonymous]
    public async Task<IActionResult> GetWeekly([FromQuery] int limit = 50)
    {
        if (limit is < 1 or > 100)
        {
            return BadRequest(new { message = "limit phải từ 1 đến 100" });
        }

        var entries = await _leaderboardService.GetWeeklyLeaderboardAsync(limit);
        return Ok(new
        {
            periodKey = Application.Helpers.VietnamTimeHelper.GetWeeklyPeriodKey(),
            entries
        });
    }
}
