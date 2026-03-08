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
    public string ISBN { get; set; } = string.Empty;

    [Range(1000, 2100, ErrorMessage = "Please enter a valid published year.")]
    [Display(Name = "Published Year")]
    public int PublishedYear { get; set; }

    [Display(Name = "Available")]
    public bool IsAvailable { get; set; } = true;
}
