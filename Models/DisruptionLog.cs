namespace AirlineOpsFlightDisruptionLogger.Models;

public class DisruptionLog
{
    public int Id { get; set; }

    public string FlightNumber { get; set; } = string.Empty;

    public int DisruptionTypeId { get; set; }

    // Filled by Dapper multi-mapping (JOIN) when logs are loaded.
    public DisruptionType DisruptionType { get; set; } = new();

    public int DelayMinutes { get; set; }

    public bool RequiresPassengerHotel { get; set; }

    public bool RequiresMealVoucher { get; set; }

    public string? Remarks { get; set; }

    public DateTime LoggedAtUtc { get; set; }
}
