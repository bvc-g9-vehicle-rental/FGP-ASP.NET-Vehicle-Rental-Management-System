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
            BorrowDate = new DateTime(2026, 2, 1),
            DueDate    = new DateTime(2026, 2, 15),
            IsReturned = false   // overdue
        },
        new()
        {
            Id         = 2,
            BookId     = 3,
            ReaderId   = 2,
            BorrowDate = new DateTime(2026, 1, 10),
            DueDate    = new DateTime(2026, 1, 24),
            ReturnDate = new DateTime(2026, 1, 30), // returned 6 days late
            IsReturned = true
        },
        new()
        {
            Id         = 3,
            BookId     = 1,
            ReaderId   = 3,
            BorrowDate = new DateTime(2026, 3, 1),
            DueDate    = new DateTime(2026, 3, 15),
            IsReturned = false   // active, not overdue
        },
        new()
        {
            Id         = 4,
            BookId     = 2,
            ReaderId   = 1,
            BorrowDate = new DateTime(2026, 2, 10),
            DueDate    = new DateTime(2026, 2, 24),
            ReturnDate = new DateTime(2026, 2, 24), // returned on time
            IsReturned = true
        },
        new()
        {
            Id         = 5,
            BookId     = 5,
            ReaderId   = 2,
            BorrowDate = new DateTime(2026, 3, 5),
            DueDate    = new DateTime(2026, 3, 19),
            IsReturned = false   // active, not overdue
        },
    ];

    private static int _nextId = 6;

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
