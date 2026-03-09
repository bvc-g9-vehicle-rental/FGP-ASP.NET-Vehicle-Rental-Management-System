using Microsoft.EntityFrameworkCore;
using LMS.Models;

namespace LMS.Data;

public class LmsDbContext : DbContext
{
    public LmsDbContext(DbContextOptions<LmsDbContext> options) : base(options) { }

    public DbSet<Book> Books { get; set; }
    public DbSet<Reader> Readers { get; set; }
    public DbSet<Borrowing> Borrowings { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Book → Borrowings (one-to-many)
        modelBuilder.Entity<Borrowing>()
            .HasOne(b => b.Book)
            .WithMany(bk => bk.Borrowings)
            .HasForeignKey(b => b.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        // Reader → Borrowings (one-to-many)
        modelBuilder.Entity<Borrowing>()
            .HasOne(b => b.Reader)
            .WithMany(r => r.Borrowings)
            .HasForeignKey(b => b.ReaderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent EF from trying to store computed properties
        modelBuilder.Entity<Borrowing>()
            .Ignore(b => b.OverdueDays)
            .Ignore(b => b.OverdueFee)
            .Ignore(b => b.IsOverdue);

        modelBuilder.Entity<Book>()
            .Ignore(b => b.IsAvailable);

        // Seed data — Books (AvailableCopies reflects active borrowings below)
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "The Great Gatsby",         Author = "F. Scott Fitzgerald", ISBN = "9780743273565", Genre = "Classic Fiction",   PublishedYear = 1925, TotalCopies = 3, AvailableCopies = 2 },
            new Book { Id = 2, Title = "To Kill a Mockingbird",    Author = "Harper Lee",           ISBN = "9780061935466", Genre = "Classic Fiction",   PublishedYear = 1960, TotalCopies = 2, AvailableCopies = 2 },
            new Book { Id = 3, Title = "1984",                     Author = "George Orwell",        ISBN = "9780451524935", Genre = "Dystopian Fiction", PublishedYear = 1949, TotalCopies = 4, AvailableCopies = 3 },
            new Book { Id = 4, Title = "Clean Code",               Author = "Robert C. Martin",     ISBN = "9780132350884", Genre = "Technology",        PublishedYear = 2008, TotalCopies = 2, AvailableCopies = 2 },
            new Book { Id = 5, Title = "The Pragmatic Programmer", Author = "David Thomas",         ISBN = "9780135957059", Genre = "Technology",        PublishedYear = 1999, TotalCopies = 3, AvailableCopies = 2 }
        );

        modelBuilder.Entity<Reader>().HasData(
            new Reader { Id = 1, Name = "Alice Johnson", Email = "alice@example.com", Phone = "403-555-0101", MemberSince = new DateTime(2024, 1, 15) },
            new Reader { Id = 2, Name = "Bob Smith",     Email = "bob@example.com",   Phone = "403-555-0102", MemberSince = new DateTime(2024, 3, 20) },
            new Reader { Id = 3, Name = "Carol White",   Email = "carol@example.com", Phone = "403-555-0103", MemberSince = new DateTime(2025, 6, 10) }
        );

        // Seed Borrowings (dates relative to current date: March 8, 2026)
        modelBuilder.Entity<Borrowing>().HasData(
            // Active & overdue — Alice borrowed Gatsby on Jan 5, due Jan 19 (~48 days overdue)
            new Borrowing { Id = 1, BookId = 1, ReaderId = 1, BorrowDate = new DateTime(2026, 1, 5),  DueDate = new DateTime(2026, 1, 19), IsReturned = false, Status = "Overdue" },
            // Returned late — Bob borrowed Mockingbird Jan 15, due Jan 29, returned Feb 10 (12 days late)
            new Borrowing { Id = 2, BookId = 2, ReaderId = 2, BorrowDate = new DateTime(2026, 1, 15), DueDate = new DateTime(2026, 1, 29), ReturnDate = new DateTime(2026, 2, 10), IsReturned = true,  Status = "Returned" },
            // Active, not yet overdue — Alice borrowed 1984 on Feb 20, due Mar 20
            new Borrowing { Id = 3, BookId = 3, ReaderId = 1, BorrowDate = new DateTime(2026, 2, 20), DueDate = new DateTime(2026, 3, 20), IsReturned = false, Status = "Active" },
            // Returned on time — Carol borrowed Clean Code Dec 1, due Dec 15, returned Dec 12
            new Borrowing { Id = 4, BookId = 4, ReaderId = 3, BorrowDate = new DateTime(2025, 12, 1), DueDate = new DateTime(2025, 12, 15), ReturnDate = new DateTime(2025, 12, 12), IsReturned = true, Status = "Returned" },
            // Active & overdue — Bob borrowed Pragmatic Programmer Feb 1, due Feb 15 (~21 days overdue)
            new Borrowing { Id = 5, BookId = 5, ReaderId = 2, BorrowDate = new DateTime(2026, 2, 1), DueDate = new DateTime(2026, 2, 15), IsReturned = false, Status = "Overdue" }
        );

        // Seed admin user — username: admin, password: admin123
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", Email = "admin@lms.local", PasswordHash = "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9" }
        );
    }
}
