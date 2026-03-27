using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleRental.Models;
using VehicleRental.Repositories;

namespace VehicleRental.Controllers;

public class ReportController : Controller
{
    private readonly IVehicleRepository     _vehicleRepo;
    private readonly ICustomerRepository    _customerRepo;
    private readonly IReservationRepository _reservationRepo;
    private readonly IBillRepository        _billRepo;

    public ReportController(
        IVehicleRepository vehicleRepo,
        ICustomerRepository customerRepo,
        IReservationRepository reservationRepo,
        IBillRepository billRepo)
    {
        _vehicleRepo     = vehicleRepo;
        _customerRepo    = customerRepo;
        _reservationRepo = reservationRepo;
        _billRepo        = billRepo;
    }

    private IActionResult RequireLogin()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");
        return null!;
    }

    // GET: /Report
    public IActionResult Index(DateTime? startDate, DateTime? endDate, VehicleType? vehicleType)
    {
        var redirect = RequireLogin();
        if (redirect is not null) return redirect;

        var reservations = _reservationRepo.GetAll().ToList();
        var vehicles = _vehicleRepo.GetAll().ToList();

        if (startDate.HasValue)
            reservations = reservations.Where(r => r.StartDate.Date >= startDate.Value.Date).ToList();

        if (endDate.HasValue)
            reservations = reservations.Where(r => r.EndDate.Date <= endDate.Value.Date).ToList();

        if (vehicleType.HasValue)
            reservations = reservations
                .Where(r => r.Vehicle?.VehicleType == vehicleType.Value)
                .ToList();

        var reservationIds = reservations.Select(r => r.Id).ToHashSet();
        var bills = _billRepo.GetAll()
            .Where(b => reservationIds.Contains(b.ReservationId))
            .ToList();

        // Revenue by vehicle type
        ViewBag.RevenueByType = bills
            .Where(b => b.IsPaid && b.Reservation?.Vehicle is not null)
            .GroupBy(b => b.Reservation!.Vehicle!.VehicleType)
            .Select(g => new { Type = g.Key.ToString(), Revenue = g.Sum(b => b.TotalAmount) })
            .ToList();

        // Reservations by status
        ViewBag.ReservationsByStatus = Enum.GetValues<ReservationStatus>()
            .Select(s => new { Status = s.ToString(), Count = reservations.Count(r => r.Status == s) })
            .ToList();

        // Top customers
        ViewBag.TopCustomers = reservations
            .GroupBy(r => r.CustomerId)
            .Select(g => new
            {
                Customer   = g.First().Customer?.FullName ?? "Unknown",
                TotalTrips = g.Count()
            })
            .OrderByDescending(x => x.TotalTrips)
            .Take(5)
            .ToList();

        ViewBag.TotalRevenue   = bills.Where(b => b.IsPaid).Sum(b => b.TotalAmount);
        ViewBag.TotalReservations = reservations.Count;
        ViewBag.FleetUtilization  = vehicles.Count == 0 ? 0 :
            (double)vehicles.Count(v => !v.IsAvailable) / vehicles.Count * 100;
        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
        ViewBag.SelectedVehicleType = vehicleType;
        ViewBag.VehicleTypes = Enum.GetValues<VehicleType>()
            .Select(type => new SelectListItem(type.ToString(), type.ToString(), type == vehicleType))
            .ToList();

        return View();
    }
}
