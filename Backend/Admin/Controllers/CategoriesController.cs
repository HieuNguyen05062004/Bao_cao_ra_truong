using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? keyword)
    {
        var data = await _categoryService.GetAllAsync(keyword);
        return Ok(data);
    }

    [HttpGet("{categoryId:int}")]
    public async Task<IActionResult> GetById(int categoryId)
    {
        var data = await _categoryService.GetByIdAsync(categoryId);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Category category)
    {
        var result = await _categoryService.CreateAsync(category);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{categoryId:int}")]
    public async Task<IActionResult> Update(int categoryId, [FromBody] Category category)
    {
        var result = await _categoryService.UpdateAsync(categoryId, category);
        if (!result.Success && result.Message.Contains("Không tìm thấy"))
        {
            return NotFound(result);
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{categoryId:int}")]
    public async Task<IActionResult> Delete(int categoryId)
    {
        var result = await _categoryService.DeleteAsync(categoryId);
        if (!result.Success && result.Message.Contains("Không tìm thấy"))
        {
            return NotFound(result);
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
