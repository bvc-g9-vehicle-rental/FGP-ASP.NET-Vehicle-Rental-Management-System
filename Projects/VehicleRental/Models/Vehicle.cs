using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRental.Models;

public enum VehicleType { Sedan, SUV, Truck, Van, Convertible, Hatchback }

public class Vehicle
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Make is required.")]
    [StringLength(50, ErrorMessage = "Make cannot exceed 50 characters.")]
    public string Make { get; set; } = string.Empty;

    [Required(ErrorMessage = "Model is required.")]
    [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters.")]
    public string Model { get; set; } = string.Empty;

    [Range(1990, 2030, ErrorMessage = "Enter a valid year (1990–2030).")]
    public int Year { get; set; }

    [Required(ErrorMessage = "License plate is required.")]
    [StringLength(15, ErrorMessage = "License plate cannot exceed 15 characters.")]
    [Display(Name = "License Plate")]
    public string LicensePlate { get; set; } = string.Empty;

    [Display(Name = "Vehicle Type")]
    public VehicleType VehicleType { get; set; }

    [Range(0.01, 10000, ErrorMessage = "Daily rate must be a positive value.")]
    [Display(Name = "Daily Rate ($)")]
    public decimal DailyRate { get; set; }

    [Display(Name = "Available")]
    public bool IsAvailable { get; set; } = true;

    [NotMapped]
    public string DisplayName => $"{Year} {Make} {Model}";
}
