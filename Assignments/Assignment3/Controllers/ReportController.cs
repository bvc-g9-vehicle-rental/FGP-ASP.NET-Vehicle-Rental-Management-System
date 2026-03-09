using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

public class ReportController : Controller
{
    private readonly LmsDbContext _context;

    public ReportController(LmsDbContext context)
    {
        _context = context;
    }

    // GET: Report
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var borrowings = await _context.Borrowings
            .Include(b => b.Book)
            .Include(b => b.Reader)
            .ToListAsync();
        var books   = await _context.Books.ToListAsync();
        var readers = await _context.Readers.ToListAsync();

        ViewBag.TotalBorrowings    = borrowings.Count;
        ViewBag.ActiveBorrowings   = borrowings.Count(b => !b.IsReturned);
        ViewBag.ReturnedBorrowings = borrowings.Count(b => b.IsReturned);
        ViewBag.OverdueBorrowings  = borrowings.Count(b => b.IsOverdue);
        ViewBag.TotalOverdueFees   = borrowings.Sum(b => b.OverdueFee);
        ViewBag.OutstandingFees    = borrowings.Where(b => !b.IsReturned).Sum(b => b.OverdueFee);
        ViewBag.TotalBooks         = books.Count;
        ViewBag.AvailableBooks     = books.Count(b => b.IsAvailable);
        ViewBag.BorrowedBooks      = books.Count(b => !b.IsAvailable);
        ViewBag.TotalReaders       = readers.Count;

        var topReader = borrowings
            .GroupBy(b => b.ReaderId)
            .OrderByDescending(g => g.Count())
            .Select(g => new { ReaderId = g.Key, Count = g.Count() })
            .FirstOrDefault();
        ViewBag.TopReaderName  = topReader is not null
            ? (borrowings.FirstOrDefault(b => b.ReaderId == topReader.ReaderId)?.Reader?.Name ?? "N/A") : "N/A";
        ViewBag.TopReaderCount = topReader?.Count ?? 0;

        var topBook = borrowings
            .GroupBy(b => b.BookId)
            .OrderByDescending(g => g.Count())
            .Select(g => new { BookId = g.Key, Count = g.Count() })
            .FirstOrDefault();
        ViewBag.TopBookTitle = topBook is not null
            ? (borrowings.FirstOrDefault(b => b.BookId == topBook.BookId)?.Book?.Title ?? "N/A") : "N/A";
        ViewBag.TopBookCount = topBook?.Count ?? 0;

        return View();
    }

    // GET: Report/Borrowings
    public IActionResult Borrowings()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        PopulateDropdowns();
        return View(new BorrowingReportFilter { FromDate = DateTime.Today.AddMonths(-3), ToDate = DateTime.Today });
    }

    // POST: Report/Borrowings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Borrowings(BorrowingReportFilter filter)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var results = _context.Borrowings
            .Include(b => b.Book)
            .Include(b => b.Reader)
            .AsQueryable();

        if (filter.FromDate.HasValue)
            results = results.Where(b => b.BorrowDate.Date >= filter.FromDate.Value.Date);

        if (filter.ToDate.HasValue)
            results = results.Where(b => b.BorrowDate.Date <= filter.ToDate.Value.Date);

        if (filter.ReaderId > 0)
            results = results.Where(b => b.ReaderId == filter.ReaderId);

        if (filter.BookId > 0)
            results = results.Where(b => b.BookId == filter.BookId);

        var list = results.OrderByDescending(b => b.BorrowDate).ToList();

        if (!string.IsNullOrEmpty(filter.Status))
        {
            list = filter.Status switch
            {
                "active"   => list.Where(b => !b.IsReturned && !b.IsOverdue).ToList(),
                "returned" => list.Where(b => b.IsReturned).ToList(),
                "overdue"  => list.Where(b => b.IsOverdue).ToList(),
                _ => list
            };
        }

        ViewBag.TotalFees = list.Sum(b => b.OverdueFee);
        PopulateDropdowns(filter.ReaderId, filter.BookId);
        ViewBag.Filter = filter;

        return View("BorrowingsResult", list);
    }

    // GET: Report/Overdue
    public IActionResult Overdue()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var overdue = _context.Borrowings
            .Include(b => b.Book)
            .Include(b => b.Reader)
            .ToList()
            .Where(b => b.OverdueDays > 0)
            .OrderByDescending(b => b.OverdueDays)
            .ToList();

        ViewBag.TotalOutstanding = overdue.Where(b => !b.IsReturned).Sum(b => b.OverdueFee);
        ViewBag.TotalCollected   = overdue.Where(b => b.IsReturned).Sum(b => b.OverdueFee);
        ViewBag.DailyRate        = Borrowing.DailyOverdueFee;

        return View(overdue);
    }

    // GET: Report/Books
    public IActionResult Books()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var books      = _context.Books.ToList();
        var borrowings = _context.Borrowings.ToList();

        var borrowCounts = borrowings
            .GroupBy(b => b.BookId)
            .ToDictionary(g => g.Key, g => g.Count());

        ViewBag.BorrowCounts = borrowCounts;
        return View(books);
    }

    // GET: Report/Readers
    public IActionResult Readers()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var readers    = _context.Readers.ToList();
        var borrowings = _context.Borrowings
            .Include(b => b.Book)
            .Include(b => b.Reader)
            .ToList();

        var stats = readers.Select(r => new
        {
            Reader           = r,
            TotalBorrows     = borrowings.Count(b => b.ReaderId == r.Id),
            ActiveBorrows    = borrowings.Count(b => b.ReaderId == r.Id && !b.IsReturned),
            OverdueBorrows   = borrowings.Count(b => b.ReaderId == r.Id && b.IsOverdue),
            OutstandingFines = borrowings.Where(b => b.ReaderId == r.Id && !b.IsReturned).Sum(b => b.OverdueFee),
        }).ToList();

        return View(stats);
    }

    private void PopulateDropdowns(int readerId = 0, int bookId = 0)
    {
        var readerItems = _context.Readers
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem(r.Name, r.Id.ToString()))
            .ToList()
            .Prepend(new SelectListItem("All Readers", "0"))
            .ToList();

        var bookItems = _context.Books
            .OrderBy(b => b.Title)
            .Select(b => new SelectListItem(b.Title, b.Id.ToString()))
            .ToList()
            .Prepend(new SelectListItem("All Books", "0"))
            .ToList();

        ViewBag.ReaderItems = new SelectList(readerItems, "Value", "Text", readerId.ToString());
        ViewBag.BookItems   = new SelectList(bookItems,   "Value", "Text", bookId.ToString());
    }
}
