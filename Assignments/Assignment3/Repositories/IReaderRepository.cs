using LMS.Models;

namespace LMS.Repositories;

public interface IReaderRepository
{
    IEnumerable<Reader> GetAll();
    Reader? GetById(int id);
    void Add(Reader reader);
    void Update(Reader reader);
    void Delete(int id);
}
