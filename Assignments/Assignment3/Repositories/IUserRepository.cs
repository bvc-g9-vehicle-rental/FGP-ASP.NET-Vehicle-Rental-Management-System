using LMS.Models;

namespace LMS.Repositories;

public interface IUserRepository
{
    User? GetByUsername(string username);
    User? GetByEmail(string email);
    bool UsernameExists(string username);
    void Add(User user);
}
