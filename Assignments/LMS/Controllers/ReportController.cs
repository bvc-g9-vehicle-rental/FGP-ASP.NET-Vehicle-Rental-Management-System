using LMS.Models;
using LMS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LMS.Controllers;

public class ReportController : Controller
{
    private readonly IBorrowingRepository _borrowingRepo;
    private readonly IBookRepository      _bookRepo;
    private readonly IReaderRepository    _readerRepo;

    public ReportController(
        IBorrowingRepository borrowingRepo,
        IBookRepository bookRepo,
        IReaderRepository readerRepo)
    {
        _borrowingRepo = borrowingRepo;
        _bookRepo      = bookRepo;
        _readerRepo    = readerRepo;
    }

    // Helper: attach navigation properties to a borrowing
    private Borrowing Populate(Borrowing b)
    {
        b.Book   = _bookRepo.GetById(b.BookId);
        b.Reader = _readerRepo.GetById(b.ReaderId);
        return b;
    }

    // ── Report landing page ──────────────────────────────────────────
    // GET: Report
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var borrowings = _borrowingRepo.GetAll().Select(Populate).ToList();
        var books      = _bookRepo.GetAll().ToList();
        var readers    = _readerRepo.GetAll().ToList();

        // Summary stats for the report landing page
        ViewBag.TotalBorrowings       = borrowings.Count;
        ViewBag.ActiveBorrowings      = borrowings.Count(b => !b.IsReturned);
        ViewBag.ReturnedBorrowings    = borrowings.Count(b => b.IsReturned);
        ViewBag.OverdueBorrowings     = borrowings.Count(b => b.IsOverdue);
        ViewBag.TotalOverdueFees      = borrowings.Sum(b => b.OverdueFee);
        ViewBag.OutstandingFees       = borrowings.Where(b => !b.IsReturned).Sum(b => b.OverdueFee);
        ViewBag.TotalBooks            = books.Count;
        ViewBag.AvailableBooks        = books.Count(b => b.IsAvailable);
        ViewBag.BorrowedBooks         = books.Count(b => !b.IsAvailable);
        ViewBag.TotalReaders          = readers.Count;

        // Most active reader
        var topReader = borrowings
            .GroupBy(b => b.ReaderId)
            .OrderByDescending(g => g.Count())
            .Select(g => new { ReaderId = g.Key, Count = g.Count() })
            .FirstOrDefault();

        ViewBag.TopReaderName  = topReader is not null ? (_readerRepo.GetById(topReader.ReaderId)?.Name ?? "—") : "—";
        ViewBag.TopReaderCount = topReader?.Count ?? 0;

        // Most borrowed book
        var topBook = borrowings
            .GroupBy(b => b.BookId)
            .OrderByDescending(g => g.Count())
            .Select(g => new { BookId = g.Key, Count = g.Count() })
            .FirstOrDefault();

        ViewBag.TopBookTitle = topBook is not null ? (_bookRepo.GetById(topBook.BookId)?.Title ?? "—") : "—";
        ViewBag.TopBookCount = topBook?.Count ?? 0;

        return View();
    }

    // ── Borrowing history report ─────────────────────────────────────
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

        var results = _borrowingRepo.GetAll().Select(Populate).AsQueryable();

        // Apply filters
        if (filter.FromDate.HasValue)
            results = results.Where(b => b.BorrowDate.Date >= filter.FromDate.Value.Date);

        if (filter.ToDate.HasValue)
            results = results.Where(b => b.BorrowDate.Date <= filter.ToDate.Value.Date);

        if (filter.ReaderId > 0)
            results = results.Where(b => b.ReaderId == filter.ReaderId);

        if (filter.BookId > 0)
            results = results.Where(b => b.BookId == filter.BookId);

        results = filter.Status switch
        {
            "active"   => results.Where(b => !b.IsReturned && !b.IsOverdue),
            "returned" => results.Where(b => b.IsReturned),
            "overdue"  => results.Where(b => b.IsOverdue),
            _          => results
        };

        var list = results.OrderByDescending(b => b.BorrowDate).ToList();

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

        // All borrowings with any overdue charges (active overdue + late returns)
        var overdue = _borrowingRepo.GetAll()
            .Select(Populate)
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

        var books      = _bookRepo.GetAll().ToList();
        var borrowings = _borrowingRepo.GetAll().ToList();

        // Borrow count per book
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

        var readers    = _readerRepo.GetAll().ToList();
        var borrowings = _borrowingRepo.GetAll().Select(Populate).ToList();

        // Per-reader stats
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

    // ── Helpers ──────────────────────────────────────────────────────
    private void PopulateDropdowns(int readerId = 0, int bookId = 0)
    {
        var readerItems = _readerRepo.GetAll()
            .Select(r => new SelectListItem(r.Name, r.Id.ToString()))
            .Prepend(new SelectListItem("All Readers", "0"))
            .ToList();

        var bookItems = _bookRepo.GetAll()
            .Select(b => new SelectListItem(b.Title, b.Id.ToString()))
            .Prepend(new SelectListItem("All Books", "0"))
            .ToList();

        ViewBag.ReaderItems = new SelectList(readerItems, "Value", "Text", readerId.ToString());
        ViewBag.BookItems   = new SelectList(bookItems,   "Value", "Text", bookId.ToString());
    }
}
