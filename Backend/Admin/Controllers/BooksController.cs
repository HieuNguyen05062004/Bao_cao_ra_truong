using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] int? categoryId)
    {
        var data = await _bookService.GetAllAsync(keyword, categoryId);
        return Ok(data);
    }

    [HttpGet("{bookId}")]
    public async Task<IActionResult> GetById(string bookId)
    {
        var data = await _bookService.GetByIdAsync(bookId);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Book book)
    {
        var result = await _bookService.CreateAsync(book);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{bookId}")]
    public async Task<IActionResult> Update(string bookId, [FromBody] Book book)
    {
        var result = await _bookService.UpdateAsync(bookId, book);
        if (!result.Success && result.Message.Contains("Không tìm thấy"))
        {
            return NotFound(result);
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{bookId}")]
    public async Task<IActionResult> Delete(string bookId)
    {
        var result = await _bookService.DeleteAsync(bookId);
        if (!result.Success && result.Message.Contains("Không tìm thấy"))
        {
            return NotFound(result);
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
