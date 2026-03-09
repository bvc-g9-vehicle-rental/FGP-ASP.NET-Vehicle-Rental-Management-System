using Microsoft.EntityFrameworkCore;
using VehicleRental.Models;

namespace VehicleRental.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User>        Users        => Set<User>();
    public DbSet<Vehicle>     Vehicles     => Set<Vehicle>();
    public DbSet<Customer>    Customers    => Set<Customer>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Bill>        Bills        => Set<Bill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Precision for money columns
        modelBuilder.Entity<Vehicle>()
            .Property(v => v.DailyRate)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Bill>()
            .Property(b => b.BaseAmount)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Bill>()
            .Property(b => b.TaxRate)
            .HasColumnType("decimal(5,2)");

        modelBuilder.Entity<Bill>()
            .Property(b => b.AdditionalCharges)
            .HasColumnType("decimal(10,2)");

        // Reservation FK — restrict delete to avoid cascade cycles
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Vehicle)
            .WithMany()
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bill>()
            .HasOne(b => b.Reservation)
            .WithMany()
            .HasForeignKey(b => b.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
