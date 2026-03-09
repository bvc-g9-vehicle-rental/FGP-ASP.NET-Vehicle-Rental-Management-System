using System.ComponentModel.DataAnnotations;

namespace LMS.Models;

public class Reader
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Display(Name = "Member Since")]
    [DataType(DataType.Date)]
    public DateTime MemberSince { get; set; } = DateTime.Today;

    // Navigation property
    public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
}
