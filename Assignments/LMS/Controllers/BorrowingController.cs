using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

public class BorrowingController : Controller
{
    private readonly LmsDbContext _context;

    public BorrowingController(LmsDbContext context)
    {
        _context = context;
    }

    private void PopulateDropdowns(int? selectedBookId = null, int? selectedReaderId = null)
    {
        var books = _context.Books
            .OrderBy(b => b.Title)
            .Select(b => new { b.Id, Display = b.Title + " (" + b.AvailableCopies + " available)" })
            .ToList();
        ViewBag.BookId   = new SelectList(books, "Id", "Display", selectedBookId);
        ViewBag.ReaderId = new SelectList(_context.Readers.OrderBy(r => r.Name), "Id", "Name", selectedReaderId);
    }

    // GET: Borrowing
    public async Task<IActionResult> Index(string? searchString)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        ViewData["CurrentFilter"] = searchString;

        var borrowings = _context.Borrowings
            .Include(b => b.Book)
            .Include(b => b.Reader)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            borrowings = borrowings.Where(b =>
                (b.Book != null && b.Book.Title.Contains(searchString)) ||
                (b.Reader != null && b.Reader.Name.Contains(searchString)) ||
                b.Status.Contains(searchString));
        }

        return View(await borrowings.OrderByDescending(b => b.BorrowDate).ToListAsync());
    }

    // GET: Borrowing/Details/5
    public async Task<IActionResult> Details(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var borrowing = await _context.Borrowings
            .Include(b => b.Book)
            .Include(b => b.Reader)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (borrowing is null) return NotFound();
        return View(borrowing);
    }

    // GET: Borrowing/Create
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        PopulateDropdowns();
        return View(new Borrowing { BorrowDate = DateTime.Today, DueDate = DateTime.Today.AddDays(14) });
    }

    // POST: Borrowing/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("BookId,ReaderId,BorrowDate,DueDate,Notes")] Borrowing borrowing)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        ModelState.Remove(nameof(Borrowing.Book));
        ModelState.Remove(nameof(Borrowing.Reader));

        if (!ModelState.IsValid)
        {
            PopulateDropdowns(borrowing.BookId, borrowing.ReaderId);
            return View(borrowing);
        }

        var book = await _context.Books.FindAsync(borrowing.BookId);
        if (book is null || book.AvailableCopies < 1)
        {
            ModelState.AddModelError("BookId", "The selected book has no available copies.");
            PopulateDropdowns(borrowing.BookId, borrowing.ReaderId);
            return View(borrowing);
        }

        book.AvailableCopies--;
        borrowing.IsReturned = false;
        borrowing.Status = "Active";

        _context.Borrowings.Add(borrowing);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Borrowing record created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Borrowing/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var borrowing = await _context.Borrowings.FindAsync(id);
        if (borrowing is null) return NotFound();

        var books = _context.Books
            .OrderBy(b => b.Title)
            .Select(b => new { b.Id, Display = b.Title + " (" + b.AvailableCopies + " available)" })
            .ToList();
        ViewBag.BookId   = new SelectList(books, "Id", "Display", borrowing.BookId);
        ViewBag.ReaderId = new SelectList(_context.Readers.OrderBy(r => r.Name), "Id", "Name", borrowing.ReaderId);

        return View(borrowing);
    }

    // POST: Borrowing/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,BookId,ReaderId,BorrowDate,DueDate,ReturnDate,IsReturned,Status")] Borrowing borrowing)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        if (id != borrowing.Id) return BadRequest();

        ModelState.Remove(nameof(Borrowing.Book));
        ModelState.Remove(nameof(Borrowing.Reader));

        if (!ModelState.IsValid)
        {
            var books = _context.Books
                .OrderBy(b => b.Title)
                .Select(b => new { b.Id, Display = b.Title + " (" + b.AvailableCopies + " available)" })
                .ToList();
            ViewBag.BookId   = new SelectList(books, "Id", "Display", borrowing.BookId);
            ViewBag.ReaderId = new SelectList(_context.Readers.OrderBy(r => r.Name), "Id", "Name", borrowing.ReaderId);
            return View(borrowing);
        }

        var existing = await _context.Borrowings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        if (existing is null) return NotFound();

        // If returning the book, restore availability
        if (borrowing.IsReturned && !existing.IsReturned)
        {
            var book = await _context.Books.FindAsync(borrowing.BookId);
            if (book is not null) book.AvailableCopies++;

            if (borrowing.ReturnDate is null) borrowing.ReturnDate = DateTime.Today;
            borrowing.Status = "Returned";
        }
        else if (!borrowing.IsReturned)
        {
            borrowing.Status = borrowing.DueDate < DateTime.Today ? "Overdue" : "Active";
        }

        try
        {
            _context.Update(borrowing);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Borrowing record updated successfully.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Borrowings.AnyAsync(b => b.Id == id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Borrowing/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var borrowing = await _context.Borrowings
            .Include(b => b.Book)
            .Include(b => b.Reader)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (borrowing is null) return NotFound();
        return View(borrowing);
    }

    // POST: Borrowing/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var borrowing = await _context.Borrowings.FindAsync(id);
        if (borrowing is not null)
        {
            // Restore book availability if not yet returned
            if (!borrowing.IsReturned)
            {
                var book = await _context.Books.FindAsync(borrowing.BookId);
                if (book is not null) book.AvailableCopies++;
            }

            _context.Borrowings.Remove(borrowing);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Borrowing record deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }
}


namespace LMS.Controllers;

public class BorrowingController : Controller
{
    private readonly IBorrowingRepository _borrowingRepo;
    private readonly IBookRepository      _bookRepo;
    private readonly IReaderRepository    _readerRepo;

    public BorrowingController(
        IBorrowingRepository borrowingRepo,
        IBookRepository bookRepo,
        IReaderRepository readerRepo)
    {
        _borrowingRepo = borrowingRepo;
        _bookRepo      = bookRepo;
        _readerRepo    = readerRepo;
    }

    // Resolve navigation properties for display
    private Borrowing Populate(Borrowing b)
    {
        b.Book   = _bookRepo.GetById(b.BookId);
        b.Reader = _readerRepo.GetById(b.ReaderId);
        return b;
    }

    // Populate ViewBag dropdowns
    private void PopulateDropdowns(int? selectedBookId = null, int? selectedReaderId = null)
    {
        var availableBooks = _bookRepo.GetAll().Where(b => b.IsAvailable).ToList();
        ViewBag.BookId   = new SelectList(availableBooks, "Id", "Title", selectedBookId);
        ViewBag.ReaderId = new SelectList(_readerRepo.GetAll(), "Id", "Name", selectedReaderId);
    }

    // GET: Borrowing
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var borrowings = _borrowingRepo.GetAll().Select(Populate).ToList();
        return View(borrowings);
    }

    // GET: Borrowing/Details/5
    public IActionResult Details(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var borrowing = _borrowingRepo.GetById(id);
        if (borrowing is null) return NotFound();
        return View(Populate(borrowing));
    }

    // GET: Borrowing/Create
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        PopulateDropdowns();
        return View(new Borrowing { BorrowDate = DateTime.Today, DueDate = DateTime.Today.AddDays(14) });
    }

    // POST: Borrowing/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Borrowing borrowing)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        // Remove navigation property validation errors
        ModelState.Remove(nameof(Borrowing.Book));
        ModelState.Remove(nameof(Borrowing.Reader));

        if (!ModelState.IsValid)
        {
            PopulateDropdowns(borrowing.BookId, borrowing.ReaderId);
            return View(borrowing);
        }

        var book = _bookRepo.GetById(borrowing.BookId);
        if (book is null || !book.IsAvailable)
        {
            ModelState.AddModelError("BookId", "The selected book is not available for borrowing.");
            PopulateDropdowns(borrowing.BookId, borrowing.ReaderId);
            return View(borrowing);
        }

        book.IsAvailable = false;
        _bookRepo.Update(book);

        borrowing.IsReturned = false;
        _borrowingRepo.Add(borrowing);

        TempData["Success"] = "Borrowing record created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Borrowing/Edit/5
    public IActionResult Edit(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var borrowing = _borrowingRepo.GetById(id);
        if (borrowing is null) return NotFound();

        // When editing, include the currently borrowed book in the dropdown too
        var books = _bookRepo.GetAll()
            .Where(b => b.IsAvailable || b.Id == borrowing.BookId)
            .ToList();
        ViewBag.BookId   = new SelectList(books, "Id", "Title", borrowing.BookId);
        ViewBag.ReaderId = new SelectList(_readerRepo.GetAll(), "Id", "Name", borrowing.ReaderId);

        return View(borrowing);
    }

    // POST: Borrowing/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Borrowing borrowing)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        if (id != borrowing.Id) return BadRequest();

        ModelState.Remove(nameof(Borrowing.Book));
        ModelState.Remove(nameof(Borrowing.Reader));

        if (!ModelState.IsValid)
        {
            var books = _bookRepo.GetAll()
                .Where(b => b.IsAvailable || b.Id == borrowing.BookId)
                .ToList();
            ViewBag.BookId   = new SelectList(books, "Id", "Title", borrowing.BookId);
            ViewBag.ReaderId = new SelectList(_readerRepo.GetAll(), "Id", "Name", borrowing.ReaderId);
            return View(borrowing);
        }

        var existing = _borrowingRepo.GetById(id);
        if (existing is null) return NotFound();

        // Handle book availability changes
        if (existing.BookId != borrowing.BookId)
        {
            var oldBook = _bookRepo.GetById(existing.BookId);
            if (oldBook is not null) { oldBook.IsAvailable = true; _bookRepo.Update(oldBook); }

            var newBook = _bookRepo.GetById(borrowing.BookId);
            if (newBook is not null && newBook.IsAvailable) { newBook.IsAvailable = false; _bookRepo.Update(newBook); }
        }

        // If marked as returned, free up the book
        if (borrowing.IsReturned && !existing.IsReturned)
        {
            var book = _bookRepo.GetById(borrowing.BookId);
            if (book is not null) { book.IsAvailable = true; _bookRepo.Update(book); }

            if (borrowing.ReturnDate is null) borrowing.ReturnDate = DateTime.Today;
        }

        _borrowingRepo.Update(borrowing);
        TempData["Success"] = "Borrowing record updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Borrowing/Delete/5
    public IActionResult Delete(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var borrowing = _borrowingRepo.GetById(id);
        if (borrowing is null) return NotFound();
        return View(Populate(borrowing));
    }

    // POST: Borrowing/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var borrowing = _borrowingRepo.GetById(id);
        if (borrowing is null) return NotFound();

        // Restore book availability if not yet returned
        if (!borrowing.IsReturned)
        {
            var book = _bookRepo.GetById(borrowing.BookId);
            if (book is not null) { book.IsAvailable = true; _bookRepo.Update(book); }
        }

        _borrowingRepo.Delete(id);
        TempData["Success"] = "Borrowing record deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
