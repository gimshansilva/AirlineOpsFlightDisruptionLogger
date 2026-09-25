using System.ComponentModel.DataAnnotations;

namespace AirlineOpsFlightDisruptionLogger.ViewModels;

public class DisruptionLogInputViewModel
{
    [Required(ErrorMessage = "Flight number is required.")]
    [StringLength(10, MinimumLength = 3, ErrorMessage = "Flight number must be between 3 and 10 characters.")]
    [RegularExpression(@"^[A-Za-z]{2,3}[0-9]{1,4}$", ErrorMessage = "Enter a valid flight number such as UL302.")]
    [Display(Name = "Flight Number")]
    public string FlightNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a disruption reason.")]
    [Display(Name = "Disruption Reason")]
    public int? DisruptionTypeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Delay duration must be a positive whole number.")]
    [Display(Name = "Delay Duration (minutes)")]
    public int DelayMinutes { get; set; }

    [Display(Name = "Hotel Accommodation Required")]
    public bool RequiresPassengerHotel { get; set; }

    [Display(Name = "Meal Vouchers Issued")]
    public bool RequiresMealVoucher { get; set; }

    [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
    public string? Remarks { get; set; }
}
