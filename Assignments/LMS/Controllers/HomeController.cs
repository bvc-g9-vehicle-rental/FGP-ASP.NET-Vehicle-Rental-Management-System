using System.Diagnostics;
using LMS.Models;
using LMS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

public class HomeController : Controller
{
    private readonly IBookRepository      _bookRepo;
    private readonly IReaderRepository    _readerRepo;
    private readonly IBorrowingRepository _borrowingRepo;

    public HomeController(
        IBookRepository bookRepo,
        IReaderRepository readerRepo,
        IBorrowingRepository borrowingRepo)
    {
        _bookRepo      = bookRepo;
        _readerRepo    = readerRepo;
        _borrowingRepo = borrowingRepo;
    }

    // GET: /  (Dashboard)
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Username") is null)
            return RedirectToAction("Login", "Account");

        var borrowings = _borrowingRepo.GetAll().ToList();

        ViewBag.TotalBooks        = _bookRepo.GetAll().Count();
        ViewBag.AvailableBooks    = _bookRepo.GetAll().Count(b => b.IsAvailable);
        ViewBag.TotalReaders      = _readerRepo.GetAll().Count();
        ViewBag.ActiveBorrowings  = borrowings.Count(b => !b.IsReturned);
        ViewBag.OverdueBorrowings = borrowings.Count(b => !b.IsReturned && b.DueDate < DateTime.Today);
        ViewBag.OutstandingFees   = borrowings.Where(b => !b.IsReturned).Sum(b => b.OverdueFee);

        var recent = borrowings
            .OrderByDescending(b => b.BorrowDate)
            .Take(5)
            .Select(b => { b.Book = _bookRepo.GetById(b.BookId); b.Reader = _readerRepo.GetById(b.ReaderId); return b; })
            .ToList();

        return View(recent);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
