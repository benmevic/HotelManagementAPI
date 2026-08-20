namespace HotelManagement.API.DTOs
{
    public class CreateReservationDto
    {
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }

    public class UpdateReservationDto
    {
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }

    public class ReservationResponseDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CancelReservationResponseDto
    {
        public int ReservationId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal PenaltyAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal CurrentBalance { get; set; }
    }
}