using CadaverExquisito.API.Application.DTOs;
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Domain.Entities;
using CadaverExquisito.API.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace CadaverExquisito.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserRepository userRepo) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var userId = HttpContext.GetUserId();
        var user = new User
        {
            Id = userId,
            Name = request.Name,
            Email = request.Email,
            FcmToken = request.FcmToken
        };
        await userRepo.UpsertAsync(user);
        return Ok();
    }

    [HttpPut("fcm-token")]
    public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenRequest request)
    {
        var userId = HttpContext.GetUserId();
        await userRepo.UpdateFcmTokenAsync(userId, request.FcmToken);
        return Ok();
    }
}
