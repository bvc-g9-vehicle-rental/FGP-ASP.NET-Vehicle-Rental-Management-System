using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VehicleRental.Models;

namespace VehicleRental.Data;

public static class DbSeeder
{
    private static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexStringLower(bytes);
    }

    public static void Seed(AppDbContext db)
    {
        db.Database.Migrate();

        if (db.Users.Any()) return; // Already seeded

        db.Users.AddRange(
            new User { Username = "admin", Email = "admin@vrms.com", PasswordHash = Hash("Admin123!") }
        );

        var vehicles = new List<Vehicle>
        {
            new() { Make = "Toyota",   Model = "Camry",    Year = 2022, LicensePlate = "ABC-1234", VehicleType = VehicleType.Sedan,       DailyRate = 55.00m,  IsAvailable = true  },
            new() { Make = "Ford",     Model = "Explorer", Year = 2021, LicensePlate = "DEF-5678", VehicleType = VehicleType.SUV,         DailyRate = 75.00m,  IsAvailable = true  },
            new() { Make = "Dodge",    Model = "Ram 1500", Year = 2023, LicensePlate = "GHI-9012", VehicleType = VehicleType.Truck,       DailyRate = 90.00m,  IsAvailable = true  },
            new() { Make = "Chrysler", Model = "Pacifica", Year = 2022, LicensePlate = "JKL-3456", VehicleType = VehicleType.Van,         DailyRate = 80.00m,  IsAvailable = true  },
            new() { Make = "Ford",     Model = "Mustang",  Year = 2023, LicensePlate = "MNO-7890", VehicleType = VehicleType.Convertible, DailyRate = 110.00m, IsAvailable = true  },
            new() { Make = "Honda",    Model = "Civic",    Year = 2022, LicensePlate = "PQR-1122", VehicleType = VehicleType.Hatchback,   DailyRate = 45.00m,  IsAvailable = true  },
        };
        db.Vehicles.AddRange(vehicles);

        var customers = new List<Customer>
        {
            new() { FirstName = "Alice",  LastName = "Johnson", Email = "alice@email.com",  Phone = "403-555-0101", LicenseNumber = "DL-10023", Address = "123 Main St, Calgary, AB"    },
            new() { FirstName = "Bob",    LastName = "Smith",   Email = "bob@email.com",    Phone = "403-555-0202", LicenseNumber = "DL-20034", Address = "456 Oak Ave, Calgary, AB"     },
            new() { FirstName = "Carol",  LastName = "Williams",Email = "carol@email.com",  Phone = "403-555-0303", LicenseNumber = "DL-30045", Address = "789 Elm Rd, Calgary, AB"      },
            new() { FirstName = "Daniel", LastName = "Brown",   Email = "daniel@email.com", Phone = "403-555-0404", LicenseNumber = "DL-40056", Address = "101 Pine Blvd, Calgary, AB"   },
        };
        db.Customers.AddRange(customers);

        db.SaveChanges();

        // Add a completed reservation + paid bill for reporting demo
        var completedRes = new Reservation
        {
            CustomerId = customers[0].Id,
            VehicleId  = vehicles[0].Id,
            StartDate  = DateTime.Today.AddDays(-5),
            EndDate    = DateTime.Today.AddDays(-2),
            Status     = ReservationStatus.Completed,
            Notes      = ""
        };
        db.Reservations.Add(completedRes);
        db.SaveChanges();

        db.Bills.Add(new Bill
        {
            ReservationId     = completedRes.Id,
            BaseAmount        = vehicles[0].DailyRate * 3,
            TaxRate           = 10m,
            AdditionalCharges = 0m,
            IsPaid            = true
        });

        db.SaveChanges();
    }
}
