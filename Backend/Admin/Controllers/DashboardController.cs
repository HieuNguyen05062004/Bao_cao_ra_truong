using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public DashboardController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var data = await _statisticsService.GetDashboardStatsAsync();
        return Ok(data);
    }
}
