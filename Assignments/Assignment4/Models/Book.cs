using System.ComponentModel.DataAnnotations;

namespace Assignment4.Models;

public class Book
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Author { get; set; } = string.Empty;

    [StringLength(20)]
    public string? ISBN { get; set; }

    [StringLength(50)]
    public string? Genre { get; set; }

    [Display(Name = "Published Year")]
    public int? PublishedYear { get; set; }

    [Required, Range(1, 1000), Display(Name = "Total Copies")]
    public int TotalCopies { get; set; } = 1;

    [Required, Range(0, 1000), Display(Name = "Available Copies")]
    public int AvailableCopies { get; set; } = 1;

    // Navigation
    public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();

    // Computed (not mapped)
    public bool IsAvailable => AvailableCopies > 0;
}
