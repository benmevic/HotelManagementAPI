using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelManagement.API.Data;
using HotelManagement.API.DTOs;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return Ok(new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Balance = user.Balance
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.Name = dto.Name;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}