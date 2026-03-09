using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRental.Models;

public class Bill
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Reservation")]
    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    [Display(Name = "Base Amount")]
    public decimal BaseAmount { get; set; }

    [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100.")]
    [Display(Name = "Tax Rate (%)")]
    public decimal TaxRate { get; set; } = 10m;

    [Range(0, double.MaxValue, ErrorMessage = "Additional charges cannot be negative.")]
    [Display(Name = "Additional Charges")]
    public decimal AdditionalCharges { get; set; }

    [NotMapped]
    [Display(Name = "Tax Amount")]
    public decimal TaxAmount => (BaseAmount + AdditionalCharges) * (TaxRate / 100);

    [NotMapped]
    [Display(Name = "Total Amount")]
    public decimal TotalAmount => BaseAmount + AdditionalCharges + TaxAmount;

    [Display(Name = "Paid")]
    public bool IsPaid { get; set; }
}
