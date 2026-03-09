using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment4.Models;

public class Borrowing
{
    public int Id { get; set; }

    [Required, Display(Name = "Book")]
    public int BookId { get; set; }

    [Required, Display(Name = "Reader")]
    public int ReaderId { get; set; }

    [Required, Display(Name = "Borrow Date"), DataType(DataType.Date)]
    public DateTime BorrowDate { get; set; } = DateTime.Today;

    [Required, Display(Name = "Due Date"), DataType(DataType.Date)]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

    [Display(Name = "Return Date"), DataType(DataType.Date)]
    public DateTime? ReturnDate { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Active"; // Active | Returned | Overdue

    public bool IsReturned { get; set; } = false;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public Book? Book { get; set; }
    public Reader? Reader { get; set; }

    // Computed (not mapped to DB)
    [NotMapped]
    public int OverdueDays => (!IsReturned && DateTime.Today > DueDate)
        ? (DateTime.Today - DueDate).Days : 0;

    [NotMapped]
    public decimal OverdueFee => OverdueDays * 0.50m;

    [NotMapped]
    public bool IsOverdue => !IsReturned && DateTime.Today > DueDate;
}
