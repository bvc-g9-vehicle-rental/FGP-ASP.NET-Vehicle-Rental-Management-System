using Assignment4.Data;
using Assignment4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment4.Controllers;

public class ReaderController : Controller
{
    private readonly LmsDbContext _context;

    public ReaderController(LmsDbContext context)
    {
        _context = context;
    }

    // GET: Reader
    public async Task<IActionResult> Index(string? searchString)
    {
        ViewData["CurrentFilter"] = searchString;

        var readers = _context.Readers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            readers = readers.Where(r =>
                r.Name.Contains(searchString) ||
                r.Email.Contains(searchString) ||
                (r.Phone != null && r.Phone.Contains(searchString)));
        }

        return View(await readers.OrderBy(r => r.Name).ToListAsync());
    }

    // GET: Reader/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var reader = await _context.Readers
            .Include(r => r.Borrowings)
                .ThenInclude(b => b.Book)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (reader is null) return NotFound();
        return View(reader);
    }

    // GET: Reader/Create
    public IActionResult Create() => View(new Reader { MemberSince = DateTime.Today });

    // POST: Reader/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Email,Phone,MemberSince")] Reader reader)
    {
        if (!ModelState.IsValid) return View(reader);

        _context.Readers.Add(reader);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Reader \"{reader.Name}\" added successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Reader/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var reader = await _context.Readers.FindAsync(id);
        if (reader is null) return NotFound();
        return View(reader);
    }

    // POST: Reader/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Phone,MemberSince")] Reader reader)
    {
        if (id != reader.Id) return BadRequest();
        if (!ModelState.IsValid) return View(reader);

        try
        {
            _context.Update(reader);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Readers.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        TempData["Success"] = $"Reader \"{reader.Name}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Reader/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var reader = await _context.Readers.FirstOrDefaultAsync(m => m.Id == id);
        if (reader is null) return NotFound();
        return View(reader);
    }

    // POST: Reader/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var reader = await _context.Readers.FindAsync(id);
        if (reader is not null)
        {
            _context.Readers.Remove(reader);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Reader \"{reader.Name}\" deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
