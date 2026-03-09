using System.ComponentModel.DataAnnotations;

namespace LMS.Models;

/// <summary>Filter parameters for the borrowing history report.</summary>
public class BorrowingReportFilter
{
    [Display(Name = "From Date")]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }

    [Display(Name = "To Date")]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }

    /// <summary>all | active | returned | overdue</summary>
    [Display(Name = "Status")]
    public string Status { get; set; } = "all";

    /// <summary>0 = all readers</summary>
    [Display(Name = "Reader")]
    public int ReaderId { get; set; } = 0;

    /// <summary>0 = all books</summary>
    [Display(Name = "Book")]
    public int BookId { get; set; } = 0;
}
