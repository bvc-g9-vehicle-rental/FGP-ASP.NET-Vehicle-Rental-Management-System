using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

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
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

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
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var book = await _context.Books
            .Include(b => b.Borrowings)
                .ThenInclude(br => br.Reader)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (book is null) return NotFound();
        return View(book);
    }

    // GET: Book/Create
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        return View();
    }

    // POST: Book/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Author,ISBN,Genre,PublishedYear,TotalCopies,AvailableCopies")] Book book)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid) return View(book);

        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Book \"{book.Title}\" added successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Book/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var book = await _context.Books.FindAsync(id);
        if (book is null) return NotFound();
        return View(book);
    }

    // POST: Book/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,ISBN,Genre,PublishedYear,TotalCopies,AvailableCopies")] Book book)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        if (id != book.Id) return BadRequest();
        if (!ModelState.IsValid) return View(book);

        try
        {
            _context.Update(book);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Book \"{book.Title}\" updated successfully.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Books.AnyAsync(b => b.Id == id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Book/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
        if (book is null) return NotFound();
        return View(book);
    }

    // POST: Book/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var book = await _context.Books.FindAsync(id);
        if (book is not null)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Book \"{book.Title}\" deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }
}


    // GET: Book
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        return View(_bookRepo.GetAll());
    }

    // GET: Book/Details/5
    public IActionResult Details(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var book = _bookRepo.GetById(id);
        if (book is null) return NotFound();
        return View(book);
    }

    // GET: Book/Create
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        return View();
    }

    // POST: Book/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Book book)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid) return View(book);

        _bookRepo.Add(book);
        TempData["Success"] = $"Book \"{book.Title}\" added successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Book/Edit/5
    public IActionResult Edit(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var book = _bookRepo.GetById(id);
        if (book is null) return NotFound();
        return View(book);
    }

    // POST: Book/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Book book)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        if (id != book.Id) return BadRequest();
        if (!ModelState.IsValid) return View(book);

        _bookRepo.Update(book);
        TempData["Success"] = $"Book \"{book.Title}\" updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Book/Delete/5
    public IActionResult Delete(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var book = _bookRepo.GetById(id);
        if (book is null) return NotFound();
        return View(book);
    }

    // POST: Book/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var book = _bookRepo.GetById(id);
        if (book is null) return NotFound();

        _bookRepo.Delete(id);
        TempData["Success"] = $"Book \"{book.Title}\" deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
