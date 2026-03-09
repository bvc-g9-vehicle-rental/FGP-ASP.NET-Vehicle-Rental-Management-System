using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

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
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

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
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var reader = await _context.Readers
            .Include(r => r.Borrowings)
                .ThenInclude(b => b.Book)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (reader is null) return NotFound();
        return View(reader);
    }

    // GET: Reader/Create
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        return View();
    }

    // POST: Reader/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Email,Phone,MemberSince")] Reader reader)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid) return View(reader);

        _context.Readers.Add(reader);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Reader \"{reader.Name}\" added successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Reader/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var reader = await _context.Readers.FindAsync(id);
        if (reader is null) return NotFound();
        return View(reader);
    }

    // POST: Reader/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Phone,MemberSince")] Reader reader)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        if (id != reader.Id) return BadRequest();
        if (!ModelState.IsValid) return View(reader);

        try
        {
            _context.Update(reader);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Reader \"{reader.Name}\" updated successfully.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Readers.AnyAsync(r => r.Id == id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Reader/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var reader = await _context.Readers.FirstOrDefaultAsync(m => m.Id == id);
        if (reader is null) return NotFound();
        return View(reader);
    }

    // POST: Reader/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var reader = await _context.Readers.FindAsync(id);
        if (reader is not null)
        {
            _context.Readers.Remove(reader);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Reader \"{reader.Name}\" deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }
}


    // GET: Reader
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        return View(_readerRepo.GetAll());
    }

    // GET: Reader/Details/5
    public IActionResult Details(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var reader = _readerRepo.GetById(id);
        if (reader is null) return NotFound();
        return View(reader);
    }

    // GET: Reader/Create
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        return View();
    }

    // POST: Reader/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Reader reader)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid) return View(reader);

        _readerRepo.Add(reader);
        TempData["Success"] = $"Reader \"{reader.Name}\" added successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Reader/Edit/5
    public IActionResult Edit(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var reader = _readerRepo.GetById(id);
        if (reader is null) return NotFound();
        return View(reader);
    }

    // POST: Reader/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Reader reader)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        if (id != reader.Id) return BadRequest();
        if (!ModelState.IsValid) return View(reader);

        _readerRepo.Update(reader);
        TempData["Success"] = $"Reader \"{reader.Name}\" updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Reader/Delete/5
    public IActionResult Delete(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var reader = _readerRepo.GetById(id);
        if (reader is null) return NotFound();
        return View(reader);
    }

    // POST: Reader/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var reader = _readerRepo.GetById(id);
        if (reader is null) return NotFound();

        _readerRepo.Delete(id);
        TempData["Success"] = $"Reader \"{reader.Name}\" deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
