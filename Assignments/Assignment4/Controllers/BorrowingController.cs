using Assignment4.Data;
using Assignment4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Assignment4.Controllers;

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
        PopulateDropdowns();
        return View(new Borrowing { BorrowDate = DateTime.Today, DueDate = DateTime.Today.AddDays(14) });
    }

    // POST: Borrowing/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("BookId,ReaderId,BorrowDate,DueDate,Notes")] Borrowing borrowing)
    {
        if (!ModelState.IsValid)
        {
            PopulateDropdowns(borrowing.BookId, borrowing.ReaderId);
            return View(borrowing);
        }

        // Decrement available copies
        var book = await _context.Books.FindAsync(borrowing.BookId);
        if (book is not null && book.AvailableCopies > 0)
        {
            book.AvailableCopies--;
        }

        borrowing.Status = "Active";
        borrowing.IsReturned = false;
        _context.Borrowings.Add(borrowing);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Borrowing record created.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Borrowing/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var borrowing = await _context.Borrowings.FindAsync(id);
        if (borrowing is null) return NotFound();
        PopulateDropdowns(borrowing.BookId, borrowing.ReaderId);
        return View(borrowing);
    }

    // POST: Borrowing/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,BookId,ReaderId,BorrowDate,DueDate,ReturnDate,Status,IsReturned,Notes")] Borrowing borrowing)
    {
        if (id != borrowing.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            PopulateDropdowns(borrowing.BookId, borrowing.ReaderId);
            return View(borrowing);
        }

        try
        {
            // If marking as returned, restore available copies and set return date
            if (borrowing.IsReturned && borrowing.ReturnDate is null)
            {
                borrowing.ReturnDate = DateTime.Today;
                borrowing.Status = "Returned";
                var book = await _context.Books.FindAsync(borrowing.BookId);
                if (book is not null) book.AvailableCopies++;
            }
            else if (!borrowing.IsReturned && DateTime.Today > borrowing.DueDate)
            {
                borrowing.Status = "Overdue";
            }

            _context.Update(borrowing);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Borrowings.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        TempData["Success"] = "Borrowing record updated.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Borrowing/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
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
        var borrowing = await _context.Borrowings.FindAsync(id);
        if (borrowing is not null)
        {
            // Restore available copies if not returned
            if (!borrowing.IsReturned)
            {
                var book = await _context.Books.FindAsync(borrowing.BookId);
                if (book is not null) book.AvailableCopies++;
            }
            _context.Borrowings.Remove(borrowing);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Borrowing record deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
