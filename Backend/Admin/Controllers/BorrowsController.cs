using Core.Shared.Interfaces;
using Core.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[ApiController]
[Route("api/borrows")]
public class BorrowsController : ControllerBase
{
    private readonly IBorrowService _borrowService;

    public BorrowsController(IBorrowService borrowService)
    {
        _borrowService = borrowService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? readerId, [FromQuery] string? status)
    {
        var data = await _borrowService.GetAllTicketsAsync(readerId, status);
        return Ok(data);
    }

    [HttpGet("{ticketId:int}")]
    public async Task<IActionResult> GetById(int ticketId)
    {
        var data = await _borrowService.GetTicketByIdAsync(ticketId);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Borrow([FromBody] BorrowRequest request)
    {
        var result = await _borrowService.BorrowAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{ticketId:int}/return")]
    public async Task<IActionResult> ReturnBook(int ticketId, [FromBody] ReturnRequest request)
    {
        var result = await _borrowService.ReturnAsync(ticketId, request);
        if (!result.Success && result.Message.Contains("Không tìm thấy"))
        {
            return NotFound(result);
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
