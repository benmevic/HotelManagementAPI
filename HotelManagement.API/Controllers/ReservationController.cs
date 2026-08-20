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
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservationController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        }

        // GET: api/Reservation/mine  (giriş yapan kullanıcının kendi rezervasyonları)
        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<ReservationResponseDto>>> GetMyReservations()
        {
            var userId = GetCurrentUserId();

            var reservations = await _context.Reservations
                .Where(r => r.UserId == userId)
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    RoomId = r.RoomId,
                    UserId = r.UserId,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status.ToString(),
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(reservations);
        }

        // POST: api/Reservation
        [HttpPost]
        public async Task<ActionResult<ReservationResponseDto>> CreateReservation(CreateReservationDto dto)
        {
            if (dto.CheckOutDate <= dto.CheckInDate)
                return BadRequest(new { message = "Çıkış tarihi giriş tarihinden sonra olmalıdır." });

            if (dto.CheckInDate.Date < DateTime.UtcNow.Date)
                return BadRequest(new { message = "Geçmiş bir tarihe rezervasyon yapılamaz." });

            var userId = GetCurrentUserId();

            // Transaction başlat — oda çakışma kontrolü, bakiye düşümü ve rezervasyon kaydı
            // hep birlikte başarılı olmalı, biri başarısız olursa hiçbiri kalıcı olmamalı.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var room = await _context.Rooms.FindAsync(dto.RoomId);
                if (room == null)
                    return NotFound(new { message = "Oda bulunamadı." });

                bool hasConflict = await _context.Reservations.AnyAsync(r =>
                    r.RoomId == dto.RoomId &&
                    r.Status == ReservationStatus.Active &&
                    r.CheckInDate < dto.CheckOutDate &&
                    r.CheckOutDate > dto.CheckInDate);

                if (hasConflict)
                    return Conflict(new { message = "Oda seçilen tarih aralığında müsait değil." });

                int nights = (dto.CheckOutDate - dto.CheckInDate).Days;
                decimal totalPrice = nights * room.PricePerNight;

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return Unauthorized();

                if (user.Balance < totalPrice)
                    return BadRequest(new { message = $"Yetersiz bakiye. Gerekli: {totalPrice}, mevcut: {user.Balance}" });

                user.Balance -= totalPrice;

                var reservation = new Reservation
                {
                    RoomId = dto.RoomId,
                    UserId = userId,
                    CheckInDate = dto.CheckInDate,
                    CheckOutDate = dto.CheckOutDate,
                    TotalPrice = totalPrice,
                    Status = ReservationStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var response = new ReservationResponseDto
                {
                    Id = reservation.Id,
                    RoomId = reservation.RoomId,
                    UserId = reservation.UserId,
                    CheckInDate = reservation.CheckInDate,
                    CheckOutDate = reservation.CheckOutDate,
                    TotalPrice = reservation.TotalPrice,
                    Status = reservation.Status.ToString(),
                    CreatedAt = reservation.CreatedAt
                };

                return CreatedAtAction(nameof(GetMyReservations), response);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}