using System.ComponentModel.DataAnnotations;

namespace Assignment4.Models;

public class Reader
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20), Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [Display(Name = "Member Since")]
    [DataType(DataType.Date)]
    public DateTime MemberSince { get; set; } = DateTime.Today;

    // Navigation
    public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
}
