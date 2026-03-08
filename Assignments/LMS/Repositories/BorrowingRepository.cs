using LMS.Models;

namespace LMS.Repositories;

public class BorrowingRepository : IBorrowingRepository
{
    private static readonly List<Borrowing> _borrowings =
    [
        new()
        {
            Id         = 1,
            BookId     = 4,
            ReaderId   = 1,
            BorrowDate = new DateTime(2026, 2, 20),
            DueDate    = new DateTime(2026, 3, 6),
            IsReturned = false
        },
    ];

    private static int _nextId = 2;

    public IEnumerable<Borrowing> GetAll() => _borrowings.ToList();

    public Borrowing? GetById(int id) => _borrowings.FirstOrDefault(b => b.Id == id);

    public void Add(Borrowing borrowing)
    {
        borrowing.Id = _nextId++;
        _borrowings.Add(borrowing);
    }

    public void Update(Borrowing borrowing)
    {
        var existing = GetById(borrowing.Id);
        if (existing is null) return;

        existing.BookId     = borrowing.BookId;
        existing.ReaderId   = borrowing.ReaderId;
        existing.BorrowDate = borrowing.BorrowDate;
        existing.DueDate    = borrowing.DueDate;
        existing.ReturnDate = borrowing.ReturnDate;
        existing.IsReturned = borrowing.IsReturned;
    }

    public void Delete(int id)
    {
        var borrowing = GetById(id);
        if (borrowing is not null) _borrowings.Remove(borrowing);
    }
}
