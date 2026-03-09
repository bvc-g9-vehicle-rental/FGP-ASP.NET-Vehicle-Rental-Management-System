using System.Security.Cryptography;
using System.Text;
using LMS.Models;

namespace LMS.Repositories;

public class UserRepository : IUserRepository
{
    private static readonly List<User> _users =
    [
        // Default admin: username = admin, password = admin123
        new() { Id = 1, Username = "admin", Email = "admin@lms.com", PasswordHash = HashPassword("admin123") },
    ];

    private static int _nextId = 2;

    public User? GetByUsername(string username) =>
        _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

    public User? GetByEmail(string email) =>
        _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

    public bool UsernameExists(string username) =>
        _users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

    public void Add(User user)
    {
        user.Id = _nextId++;
        _users.Add(user);
    }

    public static bool VerifyPassword(string plainText, string hash) =>
        HashPassword(plainText) == hash;

    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
