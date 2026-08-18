namespace HotelManagement.API.Models;

public class Room
{
    public int Id { get; set; }
    public RoomType RoomType { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }

    public int HotelId { get; set; }
    public Hotel Hotel { get; set; } = null!;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}