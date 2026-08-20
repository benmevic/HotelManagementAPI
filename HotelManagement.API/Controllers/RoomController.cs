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
    public class RoomController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        }

        // GET: api/Room  (herkes görebilir)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetRooms([FromQuery] int? hotelId)
        {
            var query = _context.Rooms.AsQueryable();

            if (hotelId.HasValue)
                query = query.Where(r => r.HotelId == hotelId.Value);

            var rooms = await query
                .Select(r => new RoomResponseDto
                {
                    Id = r.Id,
                    RoomType = r.RoomType,
                    Capacity = r.Capacity,
                    PricePerNight = r.PricePerNight,
                    HotelId = r.HotelId
                })
                .ToListAsync();

            return Ok(rooms);
        }

        // GET: api/Room/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomResponseDto>> GetRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound(new { message = "Oda bulunamadı." });

            return Ok(new RoomResponseDto
            {
                Id = room.Id,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                PricePerNight = room.PricePerNight,
                HotelId = room.HotelId
            });
        }

        // POST: api/Room  (sadece otelin sahibi kendi oteline oda ekleyebilir)
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RoomResponseDto>> CreateRoom(CreateRoomDto dto)
        {
            var hotel = await _context.Hotels.FindAsync(dto.HotelId);

            if (hotel == null)
                return NotFound(new { message = "Belirtilen otel bulunamadı." });

            var userId = GetCurrentUserId();

            if (hotel.OwnerUserId != userId)
                return Forbid();

            var room = new Room
            {
                RoomType = dto.RoomType,
                Capacity = dto.Capacity,
                PricePerNight = dto.PricePerNight,
                HotelId = dto.HotelId
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var response = new RoomResponseDto
            {
                Id = room.Id,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                PricePerNight = room.PricePerNight,
                HotelId = room.HotelId
            };

            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, response);
        }

        // PUT: api/Room/5  (sadece otelin sahibi güncelleyebilir)
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateRoom(int id, UpdateRoomDto dto)
        {
            var room = await _context.Rooms.Include(r => r.Hotel).FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
                return NotFound(new { message = "Oda bulunamadı." });

            var userId = GetCurrentUserId();

            if (room.Hotel!.OwnerUserId != userId)
                return Forbid();

            room.RoomType = dto.RoomType;
            room.Capacity = dto.Capacity;
            room.PricePerNight = dto.PricePerNight;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Room/5  (sadece otelin sahibi silebilir)
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.Include(r => r.Hotel).FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
                return NotFound(new { message = "Oda bulunamadı." });

            var userId = GetCurrentUserId();

            if (room.Hotel!.OwnerUserId != userId)
                return Forbid();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Room/5/availability?checkInDate=2026-09-01&checkOutDate=2026-09-05
        [HttpGet("{id}/availability")]
        public async Task<ActionResult<RoomAvailabilityResponseDto>> CheckAvailability(
            int id, [FromQuery] DateTime checkInDate, [FromQuery] DateTime checkOutDate)
        {
            if (checkOutDate <= checkInDate)
                return BadRequest(new { message = "Çıkış tarihi giriş tarihinden sonra olmalıdır." });

            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound(new { message = "Oda bulunamadı." });

            bool hasConflict = await _context.Reservations.AnyAsync(r =>
                r.RoomId == id &&
                r.Status == ReservationStatus.Active &&
                r.CheckInDate < checkOutDate &&
                r.CheckOutDate > checkInDate);

            int nights = (checkOutDate - checkInDate).Days;
            decimal totalPrice = nights * room.PricePerNight;

            return Ok(new RoomAvailabilityResponseDto
            {
                RoomId = id,
                IsAvailable = !hasConflict,
                Nights = nights,
                TotalPrice = totalPrice
            });
        }


    }
}