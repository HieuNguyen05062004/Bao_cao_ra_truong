using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("basic")]
    public async Task<IActionResult> Basic([FromQuery] string keyword)
    {
        var data = await _searchService.BasicSearchAsync(keyword);
        return Ok(data);
    }

    [HttpGet("advanced")]
    public async Task<IActionResult> Advanced([FromQuery] string query)
    {
        var data = await _searchService.AdvancedSearchAsync(query);
        return Ok(data);
    }
}
