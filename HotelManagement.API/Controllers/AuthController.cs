using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelManagement.API.Data;
using HotelManagement.API.DTOs;
using HotelManagement.API.Models;
using HotelManagement.API.Services;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
            return BadRequest("Bu e-posta adresi zaten kayıtlı.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            Balance = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Token = token
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("E-posta veya parola hatalı.");

        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Token = token
        });
    }
}