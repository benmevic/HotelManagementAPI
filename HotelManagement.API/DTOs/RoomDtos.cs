using HotelManagement.API.Models;

namespace HotelManagement.API.DTOs
{
    public class CreateRoomDto
    {
        public RoomType RoomType { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public int HotelId { get; set; }
    }

    public class UpdateRoomDto
    {
        public RoomType RoomType { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
    }

    public class RoomResponseDto
    {
        public int Id { get; set; }
        public RoomType RoomType { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public int HotelId { get; set; }
    }

    public class RoomAvailabilityQueryDto
    {
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }

    public class RoomAvailabilityResponseDto
    {
        public int RoomId { get; set; }
        public bool IsAvailable { get; set; }
        public int Nights { get; set; }
        public decimal TotalPrice { get; set; }
    }





}