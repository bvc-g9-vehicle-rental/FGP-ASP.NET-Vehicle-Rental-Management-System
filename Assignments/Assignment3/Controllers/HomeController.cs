using System.Diagnostics;
using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

public class HomeController : Controller
{
    private readonly LmsDbContext _context;

    public HomeController(LmsDbContext context)
    {
        _context = context;
    }

    // GET: /  (Dashboard)
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        ViewBag.TotalBooks        = await _context.Books.CountAsync();
        ViewBag.AvailableBooks    = await _context.Books.SumAsync(b => b.AvailableCopies);
        ViewBag.TotalReaders      = await _context.Readers.CountAsync();
        ViewBag.ActiveBorrowings  = await _context.Borrowings.CountAsync(b => !b.IsReturned);
        ViewBag.OverdueBorrowings = await _context.Borrowings.CountAsync(b => !b.IsReturned && b.DueDate < DateTime.Today);

        // Outstanding fees require client-side evaluation (computed property)
        var activeBorrowings = await _context.Borrowings.Where(b => !b.IsReturned).ToListAsync();
        ViewBag.OutstandingFees = activeBorrowings.Sum(b => b.OverdueFee);

        var recent = await _context.Borrowings
            .Include(b => b.Book)
            .Include(b => b.Reader)
            .OrderByDescending(b => b.BorrowDate)
            .Take(5)
            .ToListAsync();

        return View(recent);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
