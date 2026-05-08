using Core.Shared.Interfaces;
using Core.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[ApiController]
[Route("api/staffs")]
public class StaffsController : ControllerBase
{
    private readonly IAuthService _authService;

    public StaffsController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _authService.GetStaffAccountsAsync();
        return Ok(data);
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetByUsername(string username)
    {
        var data = await _authService.GetStaffByUsernameAsync(username);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StaffUpsertRequest request)
    {
        var result = await _authService.CreateStaffAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{username}")]
    public async Task<IActionResult> Update(string username, [FromBody] StaffUpsertRequest request)
    {
        var result = await _authService.UpdateStaffAsync(username, request);
        if (!result.Success && result.Message.Contains("Không tìm thấy"))
        {
            return NotFound(result);
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{username}")]
    public async Task<IActionResult> Delete(string username)
    {
        var result = await _authService.DeleteStaffAsync(username);
        if (!result.Success && result.Message.Contains("Không tìm thấy"))
        {
            return NotFound(result);
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
