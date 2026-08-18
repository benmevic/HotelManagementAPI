namespace HotelManagement.API.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}