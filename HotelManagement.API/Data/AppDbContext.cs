using Microsoft.EntityFrameworkCore;
using HotelManagement.API.Models;

namespace HotelManagement.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User - Hotel (1-N): bir kullanıcı silinirse otelleri de silinmesin, engelle
        modelBuilder.Entity<Hotel>()
            .HasOne(h => h.Owner)
            .WithMany(u => u.Hotels)
            .HasForeignKey(h => h.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Hotel - Room (1-N): otel silinirse odaları da silinsin
        modelBuilder.Entity<Room>()
            .HasOne(r => r.Hotel)
            .WithMany(h => h.Rooms)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        // Room - Reservation (1-N): oda silinirse rezervasyonlar da silinmesin, engelle
        modelBuilder.Entity<Reservation>()
            .HasOne(res => res.Room)
            .WithMany(r => r.Reservations)
            .HasForeignKey(res => res.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // User - Reservation (1-N)
        modelBuilder.Entity<Reservation>()
            .HasOne(res => res.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(res => res.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Decimal alanların hassasiyeti (SQL Server için önemli, yoksa uyarı verir)
        modelBuilder.Entity<Room>().Property(r => r.PricePerNight).HasPrecision(10, 2);
        modelBuilder.Entity<User>().Property(u => u.Balance).HasPrecision(10, 2);
        modelBuilder.Entity<Reservation>().Property(r => r.TotalPrice).HasPrecision(10, 2);
    }
}