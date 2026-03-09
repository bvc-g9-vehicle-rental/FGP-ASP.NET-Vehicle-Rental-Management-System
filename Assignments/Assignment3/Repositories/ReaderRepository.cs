using LMS.Models;

namespace LMS.Repositories;

public class ReaderRepository : IReaderRepository
{
    private static readonly List<Reader> _readers =
    [
        new() { Id = 1, Name = "Alice Johnson",  Email = "alice@example.com",  Phone = "403-555-1001", MemberSince = new DateTime(2023, 1, 15) },
        new() { Id = 2, Name = "Bob Smith",      Email = "bob@example.com",    Phone = "403-555-1002", MemberSince = new DateTime(2023, 3, 20) },
        new() { Id = 3, Name = "Carol Williams", Email = "carol@example.com",  Phone = "403-555-1003", MemberSince = new DateTime(2024, 6, 5)  },
    ];

    private static int _nextId = 4;

    public IEnumerable<Reader> GetAll() => _readers.ToList();

    public Reader? GetById(int id) => _readers.FirstOrDefault(r => r.Id == id);

    public void Add(Reader reader)
    {
        reader.Id = _nextId++;
        _readers.Add(reader);
    }

    public void Update(Reader reader)
    {
        var existing = GetById(reader.Id);
        if (existing is null) return;

        existing.Name        = reader.Name;
        existing.Email       = reader.Email;
        existing.Phone       = reader.Phone;
        existing.MemberSince = reader.MemberSince;
    }

    public void Delete(int id)
    {
        var reader = GetById(id);
        if (reader is not null) _readers.Remove(reader);
    }
}
