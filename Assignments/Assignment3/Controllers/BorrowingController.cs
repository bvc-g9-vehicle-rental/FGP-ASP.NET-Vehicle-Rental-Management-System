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