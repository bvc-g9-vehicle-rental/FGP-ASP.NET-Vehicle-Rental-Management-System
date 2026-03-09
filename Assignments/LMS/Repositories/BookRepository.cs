using LMS.Models;

namespace LMS.Repositories;

public class BookRepository : IBookRepository
{
    private static readonly List<Book> _books =
    [
        new() { Id = 1, Title = "The Great Gatsby",        Author = "F. Scott Fitzgerald", ISBN = "9780743273565", PublishedYear = 1925, IsAvailable = false },
        new() { Id = 2, Title = "To Kill a Mockingbird",   Author = "Harper Lee",          ISBN = "9780061935466", PublishedYear = 1960, IsAvailable = true  },
        new() { Id = 3, Title = "1984",                    Author = "George Orwell",        ISBN = "9780451524935", PublishedYear = 1949, IsAvailable = true  },
        new() { Id = 4, Title = "The Catcher in the Rye", Author = "J.D. Salinger",        ISBN = "9780316769174", PublishedYear = 1951, IsAvailable = false },
        new() { Id = 5, Title = "Brave New World",         Author = "Aldous Huxley",        ISBN = "9780060850524", PublishedYear = 1932, IsAvailable = false },
    ];

    private static int _nextId = 6;

    public IEnumerable<Book> GetAll() => _books.ToList();

    public Book? GetById(int id) => _books.FirstOrDefault(b => b.Id == id);

    public void Add(Book book)
    {
        book.Id = _nextId++;
        _books.Add(book);
    }

    public void Update(Book book)
    {
        var existing = GetById(book.Id);
        if (existing is null) return;

        existing.Title         = book.Title;
        existing.Author        = book.Author;
        existing.ISBN          = book.ISBN;
        existing.PublishedYear = book.PublishedYear;
        existing.IsAvailable   = book.IsAvailable;
    }

    public void Delete(int id)
    {
        var book = GetById(id);
        if (book is not null) _books.Remove(book);
    }
}
