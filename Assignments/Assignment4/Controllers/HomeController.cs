using System.Diagnostics;
using Assignment4.Data;
using Assignment4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment4.Controllers;

public class HomeController : Controller
{
    private readonly LmsDbContext _context;

    public HomeController(LmsDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalBooks      = await _context.Books.CountAsync();
        ViewBag.TotalReaders    = await _context.Readers.CountAsync();
        ViewBag.ActiveBorrowings = await _context.Borrowings.CountAsync(b => !b.IsReturned);
        ViewBag.OverdueBorrowings = await _context.Borrowings
            .Include(b => b.Book)
            .Include(b => b.Reader)
            .Where(b => !b.IsReturned && b.DueDate < DateTime.Today)
            .OrderBy(b => b.DueDate)
            .ToListAsync();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

