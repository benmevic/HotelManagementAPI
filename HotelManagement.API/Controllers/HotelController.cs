using HotelManagement.API.Data;
using HotelManagement.API.DTOs;
using HotelManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HotelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HotelController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        }

        // GET: api/Hotel  (herkes görebilir, auth gerekmez)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HotelResponseDto>>> GetHotels()
        {
            var hotels = await _context.Hotels
                .Select(h => new HotelResponseDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Address = h.Address,
                    Description = h.Description,
                    OwnerUserId = h.OwnerUserId
                })
                .ToListAsync();

            return Ok(hotels);
        }

        // GET: api/Hotel/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HotelResponseDto>> GetHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);

            if (hotel == null)
                return NotFound(new { message = "Otel bulunamadı." });

            return Ok(new HotelResponseDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Address = hotel.Address,
                Description = hotel.Description,
                OwnerUserId = hotel.OwnerUserId
            });
        }

        // POST: api/Hotel  (giriş yapmış herhangi bir kullanıcı otel açabilir)
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<HotelResponseDto>> CreateHotel(CreateHotelDto dto)
        {
            var userId = GetCurrentUserId();

            var hotel = new Hotel
            {
                Name = dto.Name,
                Address = dto.Address,
                Description = dto.Description,
                OwnerUserId = userId
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            var response = new HotelResponseDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Address = hotel.Address,
                Description = hotel.Description,
                OwnerUserId = hotel.OwnerUserId
            };

            return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, response);
        }

        // PUT: api/Hotel/5  (sadece otelin sahibi güncelleyebilir)
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateHotel(int id, UpdateHotelDto dto)
        {
            var hotel = await _context.Hotels.FindAsync(id);

            if (hotel == null)
                return NotFound(new { message = "Otel bulunamadı." });

            var userId = GetCurrentUserId();

            if (hotel.OwnerUserId != userId)
                return Forbid();

            hotel.Name = dto.Name;
            hotel.Address = dto.Address;
            hotel.Description = dto.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Hotel/5  (sadece otelin sahibi silebilir)
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);

            if (hotel == null)
                return NotFound(new { message = "Otel bulunamadı." });

            var userId = GetCurrentUserId();

            if (hotel.OwnerUserId != userId)
                return Forbid();

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}