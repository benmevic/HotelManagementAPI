namespace HotelManagement.API.Models;

public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}