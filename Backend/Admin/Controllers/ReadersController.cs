using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[ApiController]
[Route("api/readers")]
public class ReadersController : ControllerBase
{
    private readonly IReaderService _readerService;

    public ReadersController(IReaderService readerService)
    {
        _readerService = readerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? keyword)
    {
        var data = await _readerService.GetAllAsync(keyword);
        return Ok(data);
    }

    [HttpGet("{readerId}")]
    public async Task<IActionResult> GetById(string readerId)
    {
        var data = await _readerService.GetByIdAsync(readerId);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Reader reader)
    {
        var result = await _readerService.CreateAsync(reader);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{readerId}")]
    public async Task<IActionResult> Update(string readerId, [FromBody] Reader reader)
    {
        var existing = await _readerService.GetByIdAsync(readerId);
        if (existing is null)
        {
            return NotFound();
        }

        var result = await _readerService.UpdateAsync(readerId, reader);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{readerId}")]
    public async Task<IActionResult> Delete(string readerId)
    {
        var existing = await _readerService.GetByIdAsync(readerId);
        if (existing is null)
        {
            return NotFound();
        }

        var result = await _readerService.DeleteAsync(readerId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
