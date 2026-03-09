using System.Security.Cryptography;
using System.Text;
using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

public class AccountController : Controller
{
    private readonly LmsDbContext _context;

    public AccountController(LmsDbContext context)
    {
        _context = context;
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    // GET: Account/Login
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("Username") is not null)
            return RedirectToAction("Index", "Home");

        return View();
    }

    // POST: Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = _context.Users.FirstOrDefault(u => u.Username.ToLower() == model.Username.ToLower());
        if (user is null || HashPassword(model.Password) != user.PasswordHash)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        return RedirectToAction("Index", "Home");
    }

    // GET: Account/Register
    public IActionResult Register()
    {
        if (HttpContext.Session.GetString("Username") is not null)
            return RedirectToAction("Index", "Home");

        return View();
    }

    // POST: Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (_context.Users.Any(u => u.Username.ToLower() == model.Username.ToLower()))
        {
            ModelState.AddModelError("Username", "This username is already taken.");
            return View(model);
        }

        if (_context.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
        {
            ModelState.AddModelError("Email", "An account with this email already exists.");
            return View(model);
        }

        var user = new User
        {
            Username     = model.Username,
            Email        = model.Email,
            PasswordHash = HashPassword(model.Password)
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        TempData["Success"] = "Account created! Please log in.";
        return RedirectToAction(nameof(Login));
    }

    // POST: Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
