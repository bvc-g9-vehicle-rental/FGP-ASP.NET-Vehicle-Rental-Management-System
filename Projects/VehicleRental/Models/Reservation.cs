using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRental.Models;

public enum ReservationStatus { Pending, Active, Completed, Cancelled }

public class Reservation
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Customer is required.")]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [Required(ErrorMessage = "Vehicle is required.")]
    [Display(Name = "Vehicle")]
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Display(Name = "Status")]
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Notes { get; set; } = string.Empty;

    [NotMapped]
    public int RentalDays => EndDate > StartDate ? (EndDate - StartDate).Days : 1;
}
