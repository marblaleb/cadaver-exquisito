using CadaverExquisito.API.Application.DTOs;
using CadaverExquisito.API.Application.Services;
using CadaverExquisito.API.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace CadaverExquisito.API.Controllers;

[ApiController]
[Route("api/cadavers")]
public class CadaversController(CadaverService cadaverService, FragmentService fragmentService) : ControllerBase
{
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        var userId = HttpContext.GetUserId();
        return Ok(await cadaverService.GetAvailableAsync(userId));
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var userId = HttpContext.GetUserId();
        return Ok(await cadaverService.GetPendingAsync(userId));
    }

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompleted() =>
        Ok(await cadaverService.GetCompletedAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCadaverRequest request)
    {
        var userId = HttpContext.GetUserId();
        var result = await cadaverService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetFull), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}/last-fragment")]
    public async Task<IActionResult> GetLastFragment(Guid id)
    {
        HttpContext.GetUserId();
        var result = await fragmentService.GetLastFragmentAsync(id);
        return result is null ? NoContent() : Ok(result);
    }

    [HttpPost("{id:guid}/fragments")]
    public async Task<IActionResult> AddFragment(Guid id, [FromBody] AddFragmentRequest request)
    {
        var userId = HttpContext.GetUserId();
        try
        {
            var result = await fragmentService.AddFragmentAsync(id, userId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}/full")]
    public async Task<IActionResult> GetFull(Guid id)
    {
        try
        {
            return Ok(await fragmentService.GetFullCadaverAsync(id));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
