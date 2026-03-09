using System.ComponentModel.DataAnnotations;

namespace LMS.Models;

public class Borrowing
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a book.")]
    [Display(Name = "Book")]
    public int BookId { get; set; }

    [Required(ErrorMessage = "Please select a reader.")]
    [Display(Name = "Reader")]
    public int ReaderId { get; set; }

    [Display(Name = "Borrow Date")]
    [DataType(DataType.Date)]
    public DateTime BorrowDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Due Date")]
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

    [Display(Name = "Return Date")]
    [DataType(DataType.Date)]
    public DateTime? ReturnDate { get; set; }

    [Display(Name = "Returned")]
    public bool IsReturned { get; set; } = false;

    // Navigation properties (resolved manually via repositories)
    public Book? Book { get; set; }
    public Reader? Reader { get; set; }

    // ── Overdue charge calculation ────────────────────────────────────
    /// <summary>Daily fine rate per overdue day.</summary>
    public static decimal DailyOverdueFee => 1.00m;

    /// <summary>
    /// Number of days the borrowing is/was overdue.
    /// Uses ReturnDate when returned; uses today when still active.
    /// </summary>
    public int OverdueDays
    {
        get
        {
            var referenceDate = (IsReturned && ReturnDate.HasValue)
                ? ReturnDate.Value.Date
                : DateTime.Today;

            var days = (int)(referenceDate - DueDate.Date).TotalDays;
            return days > 0 ? days : 0;
        }
    }

    /// <summary>Total overdue fee (OverdueDays × DailyOverdueFee).</summary>
    public decimal OverdueFee => OverdueDays * DailyOverdueFee;

    /// <summary>True when an active borrowing has passed its due date.</summary>
    public bool IsOverdue => !IsReturned && DateTime.Today > DueDate.Date;
}
