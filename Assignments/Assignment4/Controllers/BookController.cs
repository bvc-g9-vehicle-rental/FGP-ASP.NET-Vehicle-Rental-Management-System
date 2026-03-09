using Assignment4.Data;
using Assignment4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment4.Controllers;

public class BookController : Controller
{
    private readonly LmsDbContext _context;

    public BookController(LmsDbContext context)
    {
        _context = context;
    }

    // GET: Book
    public async Task<IActionResult> Index(string? searchString)
    {
        ViewData["CurrentFilter"] = searchString;

        var books = _context.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            books = books.Where(b =>
                b.Title.Contains(searchString) ||
                b.Author.Contains(searchString) ||
                (b.ISBN != null && b.ISBN.Contains(searchString)) ||
                (b.Genre != null && b.Genre.Contains(searchString)));
        }

        return View(await books.OrderBy(b => b.Title).ToListAsync());
    }

    // GET: Book/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var book = await _context.Books
            .Include(b => b.Borrowings)
                .ThenInclude(br => br.Reader)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (book is null) return NotFound();
        return View(book);
    }

    // GET: Book/Create
    public IActionResult Create() => View();

    // POST: Book/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Author,ISBN,Genre,PublishedYear,TotalCopies,AvailableCopies")] Book book)
    {
        if (!ModelState.IsValid) return View(book);

        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Book \"{book.Title}\" added successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Book/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book is null) return NotFound();
        return View(book);
    }

    // POST: Book/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,ISBN,Genre,PublishedYear,TotalCopies,AvailableCopies")] Book book)
    {
        if (id != book.Id) return BadRequest();
        if (!ModelState.IsValid) return View(book);

        try
        {
            _context.Update(book);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Books.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        TempData["Success"] = $"Book \"{book.Title}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Book/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
        if (book is null) return NotFound();
        return View(book);
    }

    // POST: Book/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book is not null)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Book \"{book.Title}\" deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
