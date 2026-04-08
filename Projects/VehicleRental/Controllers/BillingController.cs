using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using VehicleRental.Models;
using VehicleRental.Repositories;

namespace VehicleRental.Controllers;

public class BillingController : Controller
{
    private readonly IBillRepository        _billRepo;
    private readonly IReservationRepository _reservationRepo;
    private readonly IVehicleRepository     _vehicleRepo;
    private readonly ICustomerRepository    _customerRepo;
    private readonly IConfiguration         _config;

    public BillingController(
        IBillRepository billRepo,
        IReservationRepository reservationRepo,
        IVehicleRepository vehicleRepo,
        ICustomerRepository customerRepo,
        IConfiguration config)
    {
        _billRepo        = billRepo;
        _reservationRepo = reservationRepo;
        _vehicleRepo     = vehicleRepo;
        _customerRepo    = customerRepo;
        _config          = config;
    }

    private IActionResult RequireLogin()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");
        return null!;
    }

    // GET: /Billing
    public IActionResult Index()
    {
        var redirect = RequireLogin();
        if (redirect is not null) return redirect;

        return View(_billRepo.GetAll().ToList());
    }

    // GET: /Billing/Details/5
    public IActionResult Details(int id)
    {
        var redirect = RequireLogin();
        if (redirect is not null) return redirect;

        var bill = _billRepo.GetById(id);
        if (bill is null) return NotFound();

        ViewBag.StripePublishableKey = _config["Stripe:PublishableKey"];
        return View(bill);
    }

    // POST: /Billing/MarkPaid/5  — manual override (admin)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarkPaid(int id)
    {
        var redirect = RequireLogin();
        if (redirect is not null) return redirect;

        var bill = _billRepo.GetById(id);
        if (bill is null) return NotFound();

        MarkBillPaid(bill);
        TempData["Success"] = "Payment recorded. Reservation marked as completed.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Billing/Checkout/5  — Stripe Checkout Session
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(int id)
    {
        var redirect = RequireLogin();
        if (redirect is not null) return redirect;

        var bill = _billRepo.GetById(id);
        if (bill is null) return NotFound();

        if (bill.IsPaid)
        {
            TempData["Info"] = "This bill is already paid.";
            return RedirectToAction(nameof(Index));
        }

        Stripe.StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        var domain = $"{Request.Scheme}://{Request.Host}";
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount  = (long)(bill.TotalAmount * 100), // cents
                        Currency    = "cad",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name        = $"Vehicle Rental — Bill #{bill.Id}",
                            Description = $"Reservation #{bill.ReservationId} · {bill.Reservation?.Customer?.FullName}"
                        }
                    },
                    Quantity = 1
                }
            ],
            Mode       = "payment",
            SuccessUrl = $"{domain}/Billing/PaymentSuccess?billId={bill.Id}",
            CancelUrl  = $"{domain}/Billing/Details/{bill.Id}"
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return Redirect(session.Url!);
    }

    // GET: /Billing/PaymentSuccess  — Stripe redirects here after payment
    public IActionResult PaymentSuccess(int billId)
    {
        var redirect = RequireLogin();
        if (redirect is not null) return redirect;

        var bill = _billRepo.GetById(billId);
        if (bill is not null && !bill.IsPaid)
            MarkBillPaid(bill);

        TempData["Success"] = "Payment successful via Stripe! Reservation marked as completed.";
        return RedirectToAction(nameof(Index));
    }

    // Shared helper — mark bill paid and close out reservation + vehicle
    private void MarkBillPaid(Bill bill)
    {
        bill.IsPaid = true;
        _billRepo.Update(bill);

        var reservation = _reservationRepo.GetById(bill.ReservationId);
        if (reservation is not null)
        {
            reservation.Status = ReservationStatus.Completed;
            _reservationRepo.Update(reservation);

            var vehicle = _vehicleRepo.GetById(reservation.VehicleId);
            if (vehicle is not null)
            {
                vehicle.IsAvailable = true;
                _vehicleRepo.Update(vehicle);
            }
        }
    }
}
