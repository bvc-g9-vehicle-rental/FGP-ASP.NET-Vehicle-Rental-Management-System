using LMS.Models;

namespace LMS.Repositories;

public interface IBorrowingRepository
{
    IEnumerable<Borrowing> GetAll();
    Borrowing? GetById(int id);
    void Add(Borrowing borrowing);
    void Update(Borrowing borrowing);
    void Delete(int id);
}
