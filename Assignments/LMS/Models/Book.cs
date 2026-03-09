using System.ComponentModel.DataAnnotations;

namespace LMS.Models;

public class Book
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Author is required.")]
    [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters.")]
    public string Author { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "ISBN cannot exceed 20 characters.")]
    [Display(Name = "ISBN")]
    public string? ISBN { get; set; }

    [StringLength(50)]
    public string? Genre { get; set; }

    [Range(1000, 2100, ErrorMessage = "Please enter a valid published year.")]
    [Display(Name = "Published Year")]
    public int? PublishedYear { get; set; }

    [Required]
    [Range(1, 999)]
    [Display(Name = "Total Copies")]
    public int TotalCopies { get; set; } = 1;

    [Required]
    [Range(0, 999)]
    [Display(Name = "Available Copies")]
    public int AvailableCopies { get; set; } = 1;

    // Navigation property
    public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
}
